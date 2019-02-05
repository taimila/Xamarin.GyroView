using System;
using Foundation;
using UIKit;
using System.Linq;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;

namespace Taimila
{
    public class GyroView : UIView
    {
        public float MaximumPressure { get; set; } = 6;
        public float LevelWeight { get; set; } = 0.01f;

        Dictionary<int, List<UIView>> levels = new Dictionary<int, List<UIView>>();

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            var touch = touches.FirstOrDefault() as UITouch;
            Pressure(touch);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            var touch = touches.FirstOrDefault() as UITouch;
            Pressure(touch);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            Animate(duration: 0.32, 
                    delay: 0, 
                    options: UIViewAnimationOptions.AllowUserInteraction, 
                    animation: Reset, 
                    completion: null);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            Animate(duration: 0.32,
                    delay: 0,
                    options: UIViewAnimationOptions.AllowUserInteraction,
                    animation: Reset,
                    completion: null);
        }

        public void AddSubview(UIView view, int level)
        {
            if (level < 0) 
                throw new ArgumentOutOfRangeException("Level must be greater than or equal to 0");

            AddSubview(view);

            if (levels.ContainsKey(level))
                levels[level].Add(view);
            else
                levels.Add(level, new List<UIView> { view });
        }

        void Pressure(UITouch touch)
        {
            var location = touch.LocationInView(this);
            Pressure(location.X, location.Y);
        }

        void Pressure(double x, double y)
        {
            double xRelative = x - Frame.Width / 2;
            x = xRelative / (Frame.Width / 2);

            double yRelative = -(y - Frame.Height / 2);
            y = yRelative / (Frame.Height / 2);

            double distance = Math.Sqrt(Math.Pow(xRelative, 2) + Math.Pow(yRelative, 2));
            double pressure = Math.Min(MaximumPressure * distance / Math.Max(Frame.Width, Frame.Height), MaximumPressure);
            
            Skew(pressure, x, y);
        }

        void Skew(double rotation, double x, double y)
        {
            var rot = (nfloat)(rotation * (Math.PI / 180));

            var transform = CATransform3D.Identity;
            transform.m34 = 1.0f / -500;
            transform = transform.Rotate(rot, (nfloat)y, (nfloat)x, 0)
                                 .Scale(0.99f, 0.99f, 1);

            PositionLevels(x, y);

            Layer.Transform = transform;
        }

        void PositionLevels(double x, double y)
        {
            foreach(var level in levels)
            {
                var views = level.Value;

                x = x * Frame.Width / 2 * ((float)level.Key) * LevelWeight;
                y = -y * Frame.Height / 2 * ((float)level.Key) * LevelWeight;

                foreach (var view in views)
                    view.Transform = CGAffineTransform.MakeTranslation((nfloat)x,(nfloat)y);
            }
        }

        void Reset()
        {
            Layer.Transform = CATransform3D.Identity;

            foreach(var view in levels.SelectMany(x => x.Value).ToList())
                view.Transform = CGAffineTransform.MakeIdentity();
        }
    }
}