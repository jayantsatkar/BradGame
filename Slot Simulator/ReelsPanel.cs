using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using System.ComponentModel;
using System.Threading;
using System.Drawing;

namespace Slot_Simulator
{
    class ReelsPanel : Panel
    {
        enum PartialSymbolPosition { Top, Bottom };
        enum WinFrameTypes { Lit, UnLit, None };
        internal Device Device;
        private Sprite m_sprite;
        private List<int> m_dimensions;
        private int m_reelCount;
        internal List<Point> ReelOffsets;
        private GameInfo m_gameInfo;
        internal List<int> SymbolOffsets;
        internal List<List<string>> SymbolsOnReel;
        internal bool Stopped = true;
        private List<List<WinFrameTypes>> m_winFrames;
        private int m_winAnimationIndex = -1;
        //Animations
        private List<AnimationPlayInstance> m_animationPlayInstances;
        internal bool HasAnimationsToPlay { get { return m_animationPlayInstances.Count > 0; } }
        internal bool HasCountUp
        {
            get
            {
                foreach (NumberBox numberBox in NumberBoxes)
                    if (numberBox.IsCounting)
                        return true;
                return false;
            }
        }
        internal bool InvalidateOverrideToggle = false;
        //Number Boxes
        internal List<NumberBox> NumberBoxes;
        internal ReelsPanel(Control _parent)
            : base()
        {
            SetStyle(ControlStyles.UserPaint, true);
            Parent = _parent;
            Name = "reelPanel";
            InitializeDevice();
            ReelImages.Initialize(Device);
            m_animationPlayInstances = new List<AnimationPlayInstance>();
            NumberBoxes = new List<NumberBox>();
        }
        ~ReelsPanel()
        {
            m_sprite.Dispose();
            Device.Dispose();
        }
        private void InitializeDevice()
        {
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Copy;
            Device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, presentParams);
            Device.SetRenderState(RenderStates.AlphaBlendEnable, true);
            m_sprite = new Sprite(Device);
        }
        new public void Invalidate()
        {
            if (!InvalidateOverrideToggle)
                base.Invalidate();
        }
        internal void InvalidateOverride()
        {
            base.Invalidate();
        }
        internal void SetGameInfo(GameInfo _gameInfo)
        {
            m_gameInfo = _gameInfo;
            m_dimensions = m_gameInfo.Dimensions;
            m_reelCount = m_dimensions.Count;
            SymbolsOnReel = new List<List<string>>();
            foreach (int dimension in m_dimensions)
                SymbolsOnReel.Add(m.MakeNewList<string>(dimension + 1, ""));
            SymbolOffsets = m.MakeNewList<int>(m_reelCount, 0);
            ReelOffsets = new List<Point>();
            int currentOffset = m_gameInfo.ReelsStartX;
            int maxDimension = m.MaxInt(m_dimensions);
            for (int reelNum = 0; reelNum < m_reelCount; reelNum++, currentOffset += m_gameInfo.SymbolWidth + m_gameInfo.ReelSpacing)
                ReelOffsets.Add(new Point(currentOffset, m_gameInfo.ReelsStartY + (maxDimension - m_dimensions[reelNum]) * m_gameInfo.SymbolHeight / 2));
            m_winFrames = new List<List<WinFrameTypes>>();
            foreach (int dimension in m_dimensions)
                m_winFrames.Add(m.MakeNewList<WinFrameTypes>(dimension, WinFrameTypes.None));
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (this)
            {
                try
                {
                    if (m_dimensions != null)
                    {
                        Device.BeginScene();
                        m_sprite.Begin(SpriteFlags.AlphaBlend);
                        if (m_gameInfo.InFreeGames && ReelImages.Textures.ContainsKey("freespinbackground"))
                            SpriteDraw("freespinbackground", new Point(0, 0));
                        else
                            SpriteDraw("background", new Point(0, 0));
                        m_gameInfo.CustomDrawBeforeDrawReels(this, m_winFrames != null, Stopped);
                        DrawReels(false);
                        DrawReels(true);
                        m_gameInfo.CustomDrawAfterDrawReels(this, m_winFrames != null, Stopped);
                        foreach (AnimationPlayInstance animationPlayInstance in new List<AnimationPlayInstance>(m_animationPlayInstances))
                        {
                            animationPlayInstance.DrawFrame(m_sprite);
                            if (animationPlayInstance.IsDone)
                                m_animationPlayInstances.Remove(animationPlayInstance);
                        }
                        foreach (NumberBox numberBox in NumberBoxes)
                            numberBox.Refresh();
                        m_sprite.End();
                        Device.EndScene();
                        Device.Present();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Source == "Microsoft.DirectX.Direct3D")
                    {
                        InitializeDevice();
                        ReelImages.Initialize(Device);
                    }
                    else
                        MessageBox.Show(ex.ToString());
                }

            }
        }
        internal void SetWinLines(WinArgs _winArg)
        {
            if (!m.ZipPlay)
            {
                m_winAnimationIndex = -1;
                if (_winArg == null || _winArg.CellNums == null)
                    m_winFrames = null;
                else
                {
                    m_winFrames = new List<List<WinFrameTypes>>();
                    for (int reelNum = 0; reelNum < m_reelCount; reelNum++)
                    {
                        List<WinFrameTypes> winFrames = m.MakeNewList<WinFrameTypes>(m_dimensions[reelNum], WinFrameTypes.None);
                        if (reelNum < _winArg.CellNums.Count && _winArg.CellNums[reelNum].Count > 0)
                        {
                            WinFrameTypes winFrameType = WinFrameTypes.None;
                            if (m_gameInfo.m_normalEvaluationType == "powerxstream2") winFrameType = (_winArg.CellNums[reelNum].Count > 0 || _winArg.IsScatter) ? WinFrameTypes.Lit : WinFrameTypes.UnLit;
                            else winFrameType = (reelNum < _winArg.Count || _winArg.IsScatter) ? WinFrameTypes.Lit : WinFrameTypes.UnLit;
                            foreach (int cellNum in _winArg.CellNums[reelNum])
                                if (cellNum < winFrames.Count)
                                    winFrames[cellNum] = winFrameType;
                        }
                        m_winFrames.Add(winFrames);
                    }
                }
            }
        }
        internal void SetWinAnimationIndex(int _winAnimationIndex)
        {
            m_winAnimationIndex = _winAnimationIndex;
        }
        //Animation/////////////////////////////////////////////////////////////////////////////////////
        private BackgroundWorker m_bgwAnimation;
        private const int cAnimationIterationDelay = 33;
        internal void PlayAnimation(string _animationName, Point _offset = new Point(), int _delay = 0)
        {
            if (ReelImages.Animations.ContainsKey(_animationName))
                m_animationPlayInstances.Add(new AnimationPlayInstance(ReelImages.Animations[_animationName], _offset, _delay));
            StartAnimationThread();
        }
        internal void StartAnimationThread()
        {
            if (m_bgwAnimation == null)
            {
                m_bgwAnimation = new BackgroundWorker();
                m_bgwAnimation.DoWork += BGWAnimation_DoWork;
                m_bgwAnimation.RunWorkerAsync();
            }
        }
        void BGWAnimation_DoWork(object sender, DoWorkEventArgs e)
        {
            DateTime startSpinTime = DateTime.Now;
            long iterations = 1;
            InvalidateOverrideToggle = true;
            while (HasAnimationsToPlay || HasCountUp || m_gameInfo.Animating)
            {
                DateTime nextSpin = startSpinTime.AddMilliseconds(cAnimationIterationDelay * iterations);
                DateTime now = DateTime.Now;
                if (nextSpin > now)
                    Thread.Sleep(nextSpin - now);
                nextSpin = startSpinTime.AddMilliseconds(cAnimationIterationDelay * (++iterations));
                InvalidateOverride();
            }
            InvalidateOverrideToggle = false;
            m_bgwAnimation = null;
        }
        //Private Functions//////////////////////////////////////////////////////////////////////////////
        private void DrawReels(bool _drawOversized)
        {
            for (int reelNum = 0; reelNum < m_reelCount; reelNum++)
            {
                Point offset = ReelOffsets[reelNum];
                int cellNum = m_dimensions[reelNum];
                for (int i = 0; i < cellNum + 1; i++)
                {
                    string symbolName = SymbolsOnReel[reelNum][i];
                    int maxHeight = cellNum * m_gameInfo.SymbolHeight;
                    int maxOffsetY = maxHeight - m_gameInfo.SymbolHeight;
                    if (_drawOversized == m_gameInfo.OversizedOffsets.ContainsKey(symbolName))
                    {
                        int offsetY = i * m_gameInfo.SymbolHeight + SymbolOffsets[reelNum];
                        if (offsetY < 0)
                        {
                            if (offsetY > -m_gameInfo.SymbolHeight)
                            {
                                int showHeight = m_gameInfo.SymbolHeight + offsetY;
                                Rectangle srcRect = new Rectangle(0, m_gameInfo.SymbolHeight - showHeight, m_gameInfo.SymbolWidth, showHeight);
                                Rectangle destRect = new Rectangle(0, 0, m_gameInfo.SymbolWidth, showHeight);
                                Point dest = new Point(offset.X, offset.Y);
                                SpriteDraw(symbolName, srcRect, destRect, dest, PartialSymbolPosition.Bottom);
                            }
                        }
                        else if (offsetY > maxOffsetY)
                        {
                            if (offsetY < maxHeight + m_gameInfo.SymbolHeight)
                            {
                                int showHeight = maxHeight - offsetY;
                                Rectangle srcRect = new Rectangle(0, 0, m_gameInfo.SymbolWidth, showHeight);
                                Rectangle destRect = new Rectangle(0, 0, m_gameInfo.SymbolWidth, showHeight);
                                Point dest = new Point(offset.X, offset.Y + offsetY);
                                SpriteDraw(symbolName, srcRect, destRect, dest, PartialSymbolPosition.Top);
                            }
                        }
                        else
                        {
                            bool tryToDisplayWinAnimation = false;
                            SpriteDraw(symbolName, new Point(offset.X, offset.Y + offsetY), tryToDisplayWinAnimation);
                        }
                        if (SymbolOffsets[reelNum] == 0 && m_winFrames != null && i < m_winFrames[reelNum].Count)
                            switch (m_winFrames[reelNum][i])
                            {
                                case WinFrameTypes.Lit: SpriteDraw("winframe", new Point(offset.X, offset.Y + offsetY)); break;
                                case WinFrameTypes.UnLit: SpriteDraw("winframe2", new Point(offset.X, offset.Y + offsetY)); break;
                            }
                    }
                }

            }
        }
        private string CharacterToBitmapName(char _char, string _prefix = "betfont")
        {
            string charName = _char.ToString();
            switch (_char)
            {
                case '.': charName = _prefix + "period"; break;
                case ',': charName = _prefix + "comma"; break;
                case '$': charName = _prefix + "dollar"; break;
                case ' ': charName = _prefix + "space"; break;
                default: charName = _prefix + _char; break;
            }
            if (!ReelImages.Bitmaps.ContainsKey(charName))
                charName = _prefix + "question";
            return charName;
        }
        internal void TextDraw(string _text, Point _dest, string _prefix = "betfont")
        {
            foreach (char character in _text.ToLower())
            {
                string charName = CharacterToBitmapName(character, _prefix);
                if (ReelImages.Bitmaps.ContainsKey(charName))
                {
                    SpriteDraw(charName, _dest);
                    _dest.X += ReelImages.Bitmaps[charName].Width;
                }
            }
        }
        internal void TextDrawAtTopCenter(string _text, Point _dest, string _prefix = "betfont")
        {
            int width = 0;
            foreach (char character in _text.ToLower())
            {
                string charName = CharacterToBitmapName(character, _prefix);
                if (ReelImages.Bitmaps.ContainsKey(charName))
                {
                    width += ReelImages.Bitmaps[charName].Width;
                }
            }
            TextDraw(_text, new Point(_dest.X - width / 2, _dest.Y), _prefix);
        }
        internal void TextDrawAtCenterCenter(string _text, Point _dest, string _prefix = "betfont")
        {
            int width = 0;
            int height = 0;
            foreach (char character in _text.ToLower())
            {
                string charName = CharacterToBitmapName(character, _prefix);
                if (ReelImages.Bitmaps.ContainsKey(charName))
                {
                    width += ReelImages.Bitmaps[charName].Width;
                    height = Math.Max(height, ReelImages.Bitmaps[charName].Height);
                }
            }
            TextDraw(_text, new Point(_dest.X - width / 2, _dest.Y - height / 2), _prefix);
        }
        internal void SpriteDraw(string _textureName, Point _dest, bool _tryToDisplayWinAnimation = false)
        {
            string originalTextureName = _textureName;
            if (_tryToDisplayWinAnimation)
            {
                for (int i = 2; i <= 5; i++)
                {
                    string animationTexture = string.Format("{0}_{1}", _textureName, m_winAnimationIndex.ToString().PadLeft(i, '0'));
                    if (ReelImages.Textures.ContainsKey(animationTexture)) _textureName = animationTexture;
                }
            }
            if (ReelImages.Textures.ContainsKey(_textureName))
            {
                Texture texture = ReelImages.Textures[_textureName];
                if (!texture.Disposed)
                {
                    if (m_gameInfo.OversizedOffsets.ContainsKey(originalTextureName))
                    {
                        _dest.X += m_gameInfo.OversizedOffsets[originalTextureName].X;
                        _dest.Y += m_gameInfo.OversizedOffsets[originalTextureName].Y;
                    }
                    m_sprite.Draw2D(texture, new Point(), 0, _dest, Color.White);
                }

            }
        }
        internal void SpriteDrawRotateMiddle(string _textureName, double _rotation, Point _dest)
        {
            if (ReelImages.Textures.ContainsKey(_textureName))
            {
                Texture texture = ReelImages.Textures[_textureName];
                if (!texture.Disposed)
                {
                    if (m_gameInfo.OversizedOffsets.ContainsKey(_textureName))
                    {
                        _dest.X += m_gameInfo.OversizedOffsets[_textureName].X;
                        _dest.Y += m_gameInfo.OversizedOffsets[_textureName].Y;
                    }
                    int width = ReelImages.Bitmaps[_textureName].Width;
                    int height = ReelImages.Bitmaps[_textureName].Height;
                    Point rotationPoint = new Point(0, 0);
                    double centerX = width / 2;
                    double centerY = height / 2;
                    double hypOfCenter = Math.Sqrt(centerX * centerX + centerY * centerY);
                    double currentCenterAngle = Math.Atan(-centerY / centerX);
                    double newCenterAngle = currentCenterAngle - _rotation;
                    double newCenterX = Math.Cos(newCenterAngle) * hypOfCenter;
                    double newCenterY = -Math.Sin(newCenterAngle) * hypOfCenter;

                    double x = _dest.X - (newCenterX - centerX);
                    double y = _dest.Y - (newCenterY - centerY);
                    double hyp = Math.Sqrt(x * x + y * y);
                    double currentAngle = Math.Atan(-y / x);
                    double previousAngle = currentAngle + _rotation;
                    double newX = Math.Cos(previousAngle) * hyp;
                    double newY = -Math.Sin(previousAngle) * hyp;
                    Point destPoint = new Point((int)newX, (int)newY);

                    m_sprite.Draw2D(texture, rotationPoint, (float)_rotation, destPoint, Color.White);
                }
            }
        }
        internal void SpriteDraw(string _textureName, Point _dest, double _xScale, double _yScale)
        {
            if (ReelImages.Textures.ContainsKey(_textureName))
            {
                Texture texture = ReelImages.Textures[_textureName];
                if (!texture.Disposed)
                {
                    if (m_gameInfo.OversizedOffsets.ContainsKey(_textureName))
                    {
                        _dest.X += m_gameInfo.OversizedOffsets[_textureName].X;
                        _dest.Y += m_gameInfo.OversizedOffsets[_textureName].Y;
                    }
                    int width = ReelImages.Bitmaps[_textureName].Width;
                    int height = ReelImages.Bitmaps[_textureName].Height;
                    int whiteColor = Color.White.ToArgb();
                    m_sprite.Draw2D(texture, Rectangle.Empty, new Rectangle(0, 0, (int)(width * _xScale), (int)(height * _yScale)), new Point((int)(_dest.X / _xScale), (int)(_dest.Y / _yScale)), whiteColor);
                }

            }
        }
        internal void SpriteDrawTopCenter(string _textureName, Point _dest)
        {
            if (ReelImages.Textures.ContainsKey(_textureName))
            {
                Texture texture = ReelImages.Textures[_textureName];
                if (!texture.Disposed)
                {
                    _dest.X -= ReelImages.Bitmaps[_textureName].Width / 2;
                    m_sprite.Draw2D(texture, new Point(), 0, _dest, Color.White);
                }

            }
        }
        internal void SpriteDrawCenterCenter(string _textureName, Point _dest)
        {
            if (ReelImages.Textures.ContainsKey(_textureName))
            {
                Texture texture = ReelImages.Textures[_textureName];
                if (!texture.Disposed)
                {
                    _dest.X -= ReelImages.Bitmaps[_textureName].Width / 2;
                    _dest.Y -= ReelImages.Bitmaps[_textureName].Height / 2;
                    m_sprite.Draw2D(texture, new Point(), 0, _dest, Color.White);
                }

            }
        }
        private void SpriteDraw(string _textureName, Rectangle _srcRect, Rectangle _destRect, Point _dest, PartialSymbolPosition _symbolPosition)
        {
            int whiteColor = Color.White.ToArgb();
            if (ReelImages.Textures.ContainsKey(_textureName))
            {
                Texture texture = ReelImages.Textures[_textureName];
                if (!texture.Disposed)
                {
                    Size size = ReelImages.Bitmaps[_textureName].Size;
                    if (m_gameInfo.OversizedOffsets.ContainsKey(_textureName))
                    {
                        Point oversizedOffsets = m_gameInfo.OversizedOffsets[_textureName];

                        switch (_symbolPosition)
                        {
                            case PartialSymbolPosition.Top:
                                _dest.X += oversizedOffsets.X;
                                _dest.Y += oversizedOffsets.Y;
                                _srcRect.Width = size.Width;
                                _srcRect.Height -= oversizedOffsets.Y;
                                _destRect.Width = size.Width;
                                _destRect.Height -= oversizedOffsets.Y;
                                //m_sprite.Draw2D(texture, _srcRect, _destRect, new Point(0, 0), 0, _dest, whiteColor);
                                break;
                            case PartialSymbolPosition.Bottom:
                                _dest.X += oversizedOffsets.X;
                                int increasedHeight = size.Height - m_gameInfo.SymbolHeight + oversizedOffsets.Y;
                                _srcRect.Y -= oversizedOffsets.Y;
                                _srcRect.Width = size.Width;
                                _srcRect.Height += increasedHeight;
                                _destRect.Width = size.Width;
                                _destRect.Height += increasedHeight;
                                m_sprite.Draw2D(texture, _srcRect, _destRect, new Point(0, 0), 0, _dest, whiteColor);
                                break;
                        }
                    }
                    else
                        m_sprite.Draw2D(texture, _srcRect, _destRect, new Point(0, 0), 0, _dest, whiteColor);
                }
            }
        }
    }
}
