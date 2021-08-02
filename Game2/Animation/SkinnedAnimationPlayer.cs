using Microsoft.Xna.Framework;
using SkinnedModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game2
{
    public class SkinnedAnimationPlayer
    {
        SkinningData skinningData;

        public AnimationClip CurrentClip { get; private set; }

        public bool Done { get; private set; }

        // Timing values
        TimeSpan startTime, endTime, currentTime;
        bool loop;
        int currentKeyframe;


        public Matrix[] BoneTransforms { get; private set; }
        public Matrix[] WorldTransforms { get; private set; }

        public Matrix[] SkinTransforms { get; private set; }

        public SkinnedAnimationPlayer(SkinningData skinningData)
        {
            this.skinningData = skinningData;

            BoneTransforms = new Matrix[skinningData.BindPose.Count];
            WorldTransforms = new Matrix[skinningData.BindPose.Count];
            SkinTransforms = new Matrix[skinningData.BindPose.Count];
        }

        public void StartClip(string clip, bool loop)
        {
            AnimationClip clipVal = skinningData.AnimationClips[clip];
            StartClip(clip, TimeSpan.FromSeconds(0), clipVal.Duration, loop);
        }

        public void StartClip(string clip, int startFrame, int endFrame, bool loop)
        {
            AnimationClip clipVal = skinningData.AnimationClips[clip];

            StartClip(clip, clipVal.Keyframes[startFrame].Time, clipVal.Keyframes[endFrame].Time, loop);
        }

        public void StartClip(string clip, TimeSpan StartTime, TimeSpan EndTime, bool loop)
        {
            CurrentClip = skinningData.AnimationClips[clip];
            currentTime = TimeSpan.FromSeconds(0);

            currentKeyframe = 0;
            Done = false;
            this.startTime = StartTime;
            this.endTime = EndTime;
            this.loop = loop;
        }


        public void Update(TimeSpan time, Matrix rootTransform)
        {
            if (CurrentClip == null || Done)
                return;
            currentTime += time;

            UpdateBoneTransforms();
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }

        public void UpdateBoneTransforms()
        {
            while (currentTime >= (endTime - startTime))
            {
                if (loop)
                {
                    currentTime -= (endTime - startTime);
                    currentKeyframe = 0;
                    skinningData.BindPose.CopyTo(BoneTransforms, 0);
                }
                else
                {
                    Done = true;
                    currentTime = endTime;
                    break;
                }
            }

            IList<Keyframe> keyframes = CurrentClip.Keyframes;

            while (currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyframe];

                if (keyframe.Time > currentTime + startTime)
                    break;

                BoneTransforms[keyframe.Bone] = keyframe.Transform;

                currentKeyframe++;
            }
        }

        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            WorldTransforms[0] = BoneTransforms[0] * rootTransform;

            for (int bone = 1; bone < WorldTransforms.Length; bone++)
            {
                int parentBone = skinningData.SkeletonHierarchy[bone];
                WorldTransforms[bone] = BoneTransforms[bone] * WorldTransforms[parentBone];
            }
        }

        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < SkinTransforms.Length; bone++)
            {
                SkinTransforms[bone] = skinningData.InverseBindPose[bone] * WorldTransforms[bone];
            }
        }
    }
}
