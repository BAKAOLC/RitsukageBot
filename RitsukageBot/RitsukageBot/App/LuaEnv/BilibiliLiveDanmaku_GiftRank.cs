using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv
{
    class BilibiliLiveDanmaku_GiftRank : INotifyPropertyChanged
    {
        private string _userName;
        private decimal _coin;
        private int _uid;

        /// <summary>
        /// 用戶名
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        /// <summary>
        /// 花销
        /// </summary>
        public decimal Coin
        {
            get { return _coin; }
            set
            {
                if (value == _coin) return;
                _coin = value;
                OnPropertyChanged(nameof(Coin));
            }
        }

        /// <summary>
        /// UID
        /// </summary>
        public int UID
        {
            get { return _uid; }
            set
            {
                if (value == _uid) return;
                _uid = value;
                OnPropertyChanged(nameof(UID));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
