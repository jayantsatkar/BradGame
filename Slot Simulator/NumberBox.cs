using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Slot_Simulator
{
    class NumberBox
    {
        bool m_countUp, m_countDown;
        int m_value, m_currentValue;
        int m_step = 10;
        Point m_centerTop;
        internal string FontPrefix;
        internal bool Show = true;
        bool m_setForProgressiveMeter;
        internal NumberBox(int _centerX, int _topY, string _fontPrefix = "betfont", bool _countUp = false, bool _countDown = false, bool _setForProgressiveMeter = false)
        {
            SetCenterTop(_centerX, _topY);
            m_value = 0;
            m_currentValue = 0;
            m_countUp = _countUp;
            m_countDown = _countDown;
            FontPrefix = _fontPrefix;
            m_setForProgressiveMeter = _setForProgressiveMeter;
        }
        internal bool IsCounting { get { return m_currentValue != m_value; } }
        internal void SetCenterTop(int _centerX, int _topY)
        {
            m_centerTop = new Point(_centerX, _topY);
        }
        internal void SetValue(int _value, bool _countTo = false)
        {
            m_value = _value;
            if (!_countTo)
                m_currentValue = _value;
            MaybeSetUpCount();
        }
        internal void ChangeValue(int _change)
        {
            m_value += _change;
            MaybeSetUpCount();
        }
        internal void ChangeValueTo(int _toValue)
        {
            m_value = _toValue;
            MaybeSetUpCount();
        }
        private void MaybeSetUpCount()
        {
            if ((m_countUp && m_value > m_currentValue) || (m_countDown && m_value < m_currentValue))
            {
                int difference = Math.Abs(m_value - m_currentValue);
                m_step = Math.Max(1, difference / 29);
                m.ReelsPanel.StartAnimationThread();
            }
            else
            {
                m_currentValue = m_value;
                m_step = 0;
            }
        }
        internal void Refresh()
        {
            if(m_currentValue < m_value)
                m_currentValue = Math.Min(m_value, m_currentValue + m_step);
            else if(m_currentValue > m_value)
                m_currentValue = Math.Max(m_value, m_currentValue - m_step);
            try
            {
                if (Show)
                {
                    if (m_setForProgressiveMeter)
                        m.ReelsPanel.TextDrawAtTopCenter(string.Format("{0:C}", (double)m_currentValue / 100), m_centerTop, FontPrefix);
                    else
                        m.ReelsPanel.TextDrawAtTopCenter(string.Format("{0}", m_currentValue), m_centerTop, FontPrefix);
                }
            }
            catch { }
        }
    }
}
