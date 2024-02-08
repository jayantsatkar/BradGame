using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace Slot_Simulator
{
    class Animation
    {
        internal List<Texture> Frames;
        internal List<Point> Offsets;

        internal Animation(List<Texture> _frames, List<Point> _offsets)
        {
            Frames = _frames;
            Offsets = _offsets;
        }
    }
    class AnimationPlayInstance
    {
        Animation m_animation;
        int m_onFrame;
        Point m_offset;
        int m_delayLeft;
        internal AnimationPlayInstance(Animation _animation, Point _offset, int _delay)
        {
            m_onFrame = 0;
            m_animation = _animation;
            m_offset = _offset;
            m_delayLeft = _delay;
        }
        internal bool IsDone { get { return m_onFrame >= m_animation.Frames.Count; } }

        internal void DrawFrame(Sprite _sprite)
        {
            if (m_delayLeft > 0)
            {
                m_delayLeft--;
                return;
            }
            Point destination = new Point(m_animation.Offsets[m_onFrame].X + m_offset.X, m_animation.Offsets[m_onFrame].Y + m_offset.Y);
            _sprite.Draw2D(m_animation.Frames[m_onFrame], new Point(), 0, destination, Color.White);
            m_onFrame++;
        }
    }
}
