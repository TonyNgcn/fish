﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using URWPGSim2D.Common;
using URWPGSim2D.StrategyLoader;
using URWPGSim2D.StrategyHelper;

namespace URWPGSim2D.Strategy
{
    public class Strategy : MarshalByRefObject, IStrategy
    {
        #region reserved code never be changed or removed
        /// <summary>
        /// override the InitializeLifetimeService to return null instead of a valid ILease implementation
        /// to ensure this type of remote object never dies
        /// </summary>
        /// <returns>null</returns>
        public override object InitializeLifetimeService()
        {
            //return base.InitializeLifetimeService();
            return null; // makes the object live indefinitely
        }
        #endregion

        /// <summary>
        /// 决策类当前对象对应的仿真使命参与队伍的决策数组引用 第一次调用GetDecision时分配空间
        /// </summary>
        /// 

        private Decision[] decisions = null;

        /// <summary>
        /// 获取队伍名称 在此处设置参赛队伍的名称
        /// </summary>
        /// <returns>队伍名称字符串</returns>
        public string GetTeamName()
        {
            return "Team First";
        }
        public int IsDirectionRight(float a, float b)
        {
            float deltaAngle = a - b;
            if (deltaAngle > Math.PI) deltaAngle -= (float)(2 * Math.PI);
            if (deltaAngle > Math.PI) deltaAngle += (float)(2 * Math.PI);

            if (deltaAngle > 1.3) return 1;//a在b右边
            else if (deltaAngle < -1.3) return -1; //a在b左边
            else return 0;
        }
        public float CalAngle(Vector3 presentPoint,Vector3 destPoint)
        {
            float angle;
            angle =MathHelper.ToRadians( Helpers.GetAngleDegree(destPoint - presentPoint));

            if (angle > Math.PI) angle -= (float)(2 * Math.PI);
            if (angle > Math.PI) angle += (float)(2 * Math.PI);
            return angle;
        }
        public Vector3 CalPointOnBall(Vector3 ball,float targetDirection)
        {
            double x = ball.X - Math.Cos(targetDirection) *58;
            double z = ball.Z - Math.Sin(targetDirection) *58;
            Vector3 point = new Vector3((float)x, 0, (float)z);
            return point;
        }
        public Vector3 CalPointOnBallR(Vector3 ball, float targetDirection)
        {
            double x = ball.X + Math.Cos(targetDirection) * 58;
            double z = ball.Z - Math.Sin(targetDirection) * 58;
            Vector3 point = new Vector3((float)x, 0, (float)z);
            return point;
        }
        public float GetVectorDistance(xna.Vector3 a, xna.Vector3 b)
        {
            return (float)Math.Sqrt((Math.Pow((a.X - b.X), 2d) + Math.Pow((a.Z - b.Z), 2d)));
        }
        public void OneFishGetScore(RoboFish fish,int leftOrRight,Vector3 ball,ref Decision decision)//leftOrRight参数：1为left，2为right
        {
            Vector3 fishLocation = fish.PositionMm;
            float fishDirection = fish.BodyDirectionRad;
            Vector3 fishTailLocation = (fish.PolygonVertices[3]+fish.PolygonVertices[4])/2;
            Vector3 fishMiddleLocation = (fish.PolygonVertices[0] + fishTailLocation) / 2;
            Vector3 targetPoint;
            float targetDirection;
            if (leftOrRight == 1)//自己球门在左边
            {
                Vector3 upPoint = new Vector3(-1000, 0, -500);
                Vector3 bottomPoint = new Vector3(-1000, 0, 500);
                Vector3 upTempPoint = new Vector3(-1350, 0, -750);
                Vector3 bottomTempPoint = new Vector3(-1350, 0, 750);
                if (ball.X > -1000) //球在门外
                {
                    if (ball.Z > 0)//距离上面的点近一点
                    {
                        targetDirection = CalAngle(ball, upTempPoint);
                    }
                    else
                    {
                        targetDirection = CalAngle(ball, bottomTempPoint);
                    }
                    targetPoint = CalPointOnBall(ball, targetDirection);
                    if (GetVectorDistance(ball, fishLocation) > 150)//快速游到目标点
                        Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5f, 8f, 400, 14, 13, 15, 100, true);
                    else
                        Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2, 3, 200, 14, 13, 15, 100, true);
                }
                else if (ball.X <= -1250)//最左边区域
                {
                    if (ball.Z < -500)//左上角
                    {
                        targetDirection = (float)3.1415;
                        targetPoint = new Vector3(-1500, 0, ball.Z-80);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 15;
                            decision.VCode = 3;
                        }
                    }
                    else if (ball.Z > 500)//左下角
                    {
                        targetDirection = (float)3.1415;
                        targetPoint = new Vector3(-1500, 0, ball.Z + 80);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 0;
                            decision.VCode = 3;
                        }
                    }
                    else//左边中间区域（除球门内）
                    {
                        if (fishDirection < 0)
                        {
                            targetDirection = CalAngle(ball, upPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);
                        }
                        else
                        {
                            targetDirection = CalAngle(ball, bottomPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);
                        }
                    }

                }
                else //if (ball.X <= -1000 && ball.X > -1250) 
                {
                    if (ball.Z < -500)//卡在球门上侧的情况
                    {
                        targetDirection = (float)-1.5708;
                        targetPoint = new Vector3(ball.X+80, 0, -1000);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 15;
                            decision.VCode = 3;
                        }
                    }
                    else if (ball.Z > 500)//卡在球门下侧的情况
                    {
                        targetDirection = (float)1.5708;
                        targetPoint = new Vector3(ball.X + 80, 0, 1000);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 0;
                            decision.VCode = 3;
                        }
                    }
                    else//球门区域内
                    {
                        //差死角位置
                        if (ball.Z > 400) 
                        {
                            targetDirection = CalAngle(ball, upPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);
                            return;
                        }
                        else if (ball.Z < -400) 
                        {
                            targetDirection = CalAngle(ball, bottomPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);

                        }
                        if (fishMiddleLocation.Z > ball.Z) //鱼在球下面
                        {
                            targetDirection = 0;
                            targetPoint = new Vector3(-1010, 0, ball.Z + 80);
                            if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                                Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                            else
                            {
                                decision.TCode = 14;
                                decision.VCode = 3;
                            }
                        }
                        else//鱼在球上面
                        {
                            targetDirection = 0;
                            targetPoint = new Vector3(-1010, 0, ball.Z - 80);
                            if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                                Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                            else
                            {
                                decision.TCode = 0;
                                decision.VCode = 3;
                            }
                        }
                    }
                }
            }
            else if(leftOrRight==2)
            {
                Vector3 upPoint = new Vector3(1000, 0, -500);
                Vector3 bottomPoint = new Vector3(1000, 0, 500);
                Vector3 upTempPoint = new Vector3(1350, 0, -750);
                Vector3 bottomTempPoint = new Vector3(1350, 0, 750);
                if (ball.X < 1000) //球在门外
                {
                    if (ball.Z > 0)//距离上面的点近一点
                    {
                        targetDirection = CalAngle(ball, upTempPoint);
                    }
                    else
                    {
                        targetDirection = CalAngle(ball, bottomTempPoint);
                    }
                    targetPoint = CalPointOnBall(ball, targetDirection);
                    if (GetVectorDistance(ball, fishLocation) > 150)//快速游到目标点
                        Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5f, 8f, 400, 14, 13, 15, 100, true);
                    else
                        Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2, 3, 200, 14, 13, 15, 100, true);
                }
                else if (ball.X >= 1250)//最右边区域
                {
                    if (ball.Z < -500)//右上角
                    {
                        targetDirection = 0;
                        targetPoint = new Vector3(1500, 0, ball.Z - 80);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 0;
                            decision.VCode = 3;
                        }
                    }
                    else if (ball.Z > 500)//右下角
                    {
                        targetDirection = 0;
                        targetPoint = new Vector3(1500, 0, ball.Z + 80);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 15;
                            decision.VCode = 3;
                        }
                    }
                    else//左边中间区域（除球门内）
                    {
                        if (fishDirection < 0)
                        {
                            targetDirection = CalAngle(ball, upPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);
                        }
                        else
                        {
                            targetDirection = CalAngle(ball, bottomPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);
                        }
                    }

                }
                else //if (ball.X <= -1000 && ball.X > -1250) 
                {
                    if (ball.Z < -500)//卡在球门上侧的情况
                    {
                        targetDirection = (float)-1.5708;
                        targetPoint = new Vector3(ball.X - 80, 0, -1000);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 0;
                            decision.VCode = 3;
                        }
                    }
                    else if (ball.Z > 500)//卡在球门下侧的情况
                    {
                        targetDirection = (float)1.5708;
                        targetPoint = new Vector3(ball.X - 80, 0, 1000);
                        if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                        else
                        {
                            decision.TCode = 15;
                            decision.VCode = 3;
                        }
                    }
                    else//球门区域内
                    {
                        //差死角位置
                        if (ball.Z > 400)
                        {
                            targetDirection = CalAngle(ball, upPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);
                            return;
                        }
                        else if (ball.Z < -400)
                        {
                            targetDirection = CalAngle(ball, bottomPoint);
                            targetPoint = CalPointOnBall(ball, targetDirection);
                            Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 5, 10, 200, 10, 8, 15, 100, true);

                        }
                        if (fishMiddleLocation.Z > ball.Z) //鱼在球下面
                        {
                            targetDirection = (float)3.1415;
                            targetPoint = new Vector3(1010, 0, ball.Z + 80);
                            if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                                Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                            else
                            {
                                decision.TCode = 0;
                                decision.VCode = 3;
                            }
                        }
                        else//鱼在球上面
                        {
                            targetDirection = (float)3.1415;
                            targetPoint = new Vector3(1010, 0, ball.Z - 80);
                            if (GetVectorDistance(fishLocation, ball) > 180 || IsDirectionRight(fishDirection, targetDirection) != 0)
                                Helpers.Dribble(ref decision, fish, targetPoint, targetDirection, 2f, 5f, 100, 5, 3, 15, 100, true);
                            else
                            {
                                decision.TCode = 14;
                                decision.VCode = 3;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取当前仿真使命（比赛项目）当前队伍所有仿真机器鱼的决策数据构成的数组
        /// </summary>
        /// <param name="mission">服务端当前运行着的仿真使命Mission对象</param>
        /// <param name="teamId">当前队伍在服务端运行着的仿真使命中所处的编号 
        /// 用于作为索引访问Mission对象的TeamsRef队伍列表中代表当前队伍的元素</param>
        /// <returns>当前队伍所有仿真机器鱼的决策数据构成的Decision数组对象</returns>
        public Decision[] GetDecision(Mission mission, int teamId)
        {
            // 决策类当前对象第一次调用GetDecision时Decision数组引用为null
            if (decisions == null)
            {// 根据决策类当前对象对应的仿真使命参与队伍仿真机器鱼的数量分配决策数组空间
                decisions = new Decision[mission.CommonPara.FishCntPerTeam];
            }
            RoboFish fish1 = mission.TeamsRef[teamId].Fishes[0];
            Vector3 ball1 = mission.EnvRef.Balls[0].PositionMm;
            RoboFish fish2 = mission.TeamsRef[teamId].Fishes[1];
            Vector3 ball2 = mission.EnvRef.Balls[1].PositionMm;
            OneFishGetScore(fish1, 1, ball1, ref decisions[0]);
            OneFishGetScore(fish2, 2, ball2, ref decisions[1]);
            //Fish1.Dingqiu(mission, teamId, ref decisions);

            return decisions;
        }
    }
}
