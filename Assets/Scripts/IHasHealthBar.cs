using System;
using UnityEngine;

public interface IHasHealthBar
{
    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public class OnHealthChangedEventArgs : EventArgs{
        public float currentHealthNormalized;
    }
}
