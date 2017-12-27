﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartCar {
    public class Navigation {
        /// <summary>
        /// 当前小车方向
        /// 车头向前、向左、向右、向后
        /// </summary>
        public enum Direction {
            Front, Left, Right, Back
        }
        // 东北方向
        public static double NE = 1 / 4.0 * Math.PI;
        // 西北方向
        public static double NW = 3 / 4.0 * Math.PI;
        // 西南方向
        public static double SW = 5 / 4.0 * Math.PI;
        // 东南方向
        public static double SE = 7 / 4.0 * Math.PI;

        // 设置最大、最小前进速度（速度配置在相应文件）
        public static int maxGo = 60;
        public static int minGo = 10;
        // 设置最大、最小平移速度（速度配置在相应文件）
        public static int maxSh = 100;
        public static int minSh = 10;
        // 设置最大、最小旋转速度（速度配置在相应文件）
        public static int maxRo = 20;
        public static int minRo = 2;
        // 设置比例 (60/0.0075 = 13333)
        public static double disP = 1200;
        public static double angP = 40;   // 10/0.008  = 250

        /// <summary>
        /// 获取当前限制信息
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxVal"></param>
        /// <param name="minVal"></param>
        /// <param name="limMin"></param>
        /// <param name="limMax"></param>
        /// <returns></returns>
        public static int getLimit(int value, int maxVal, int minVal, bool limMin, bool limMax) {
            // 限制最小
            if (limMin) {
                if (value > 0) {
                    value = Math.Max(value, minVal);
                }
                else {
                    value = Math.Min(value, -minVal);
                }
            }
            // 限制最大
            if (limMax) {
                if (value > 0) {
                    value = Math.Min(value, maxVal);
                }
                else {
                    value = Math.Max(value, -maxVal);
                }
            }
            return value;
        }
        /// <summary>
        /// 获取当前角度信息
        /// </summary>
        /// <param name="originAngle">从编码器获取的车的角度</param>
        /// <returns>转换的范围内角度（0 到 2 * Math.PI）</returns>
        public static double getRangeAngle(double originAngle) {
            while (originAngle < 0) {
                originAngle += 2 * Math.PI;
            }
            while (originAngle > 2 * Math.PI) {
                originAngle -= 2 * Math.PI;
            }
            return originAngle;
        }
        /// <summary>
        /// 获取车当前的位置信息
        /// </summary>
        /// <param name="originAngle"></param>
        /// <returns></returns>
        public static Direction getDirection(double originAngle) {
            double curAngle = getRangeAngle(originAngle);
            if (NE < curAngle && curAngle <= NW) {
                return Direction.Front;
            }
            else if (NW < curAngle && curAngle <= SW) {
                return Direction.Left;
            }
            else if (SW < curAngle && curAngle <= SE) {
                return Direction.Back;
            }
            return Direction.Right;
        }

        /// <summary>
        /// 车头朝前时，AGV控制策略
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="cur"></param>
        /// <param name="goSpeed"></param>
        /// <param name="shSpeed"></param>
        public static void faceFront(KeyPoint start, KeyPoint end, KeyPoint cur, ref int goSpeed, ref int shSpeed) {
            // X方向为主要偏差
            if (Math.Abs(start.y - end.y) < Math.Abs(start.x - end.x)) {
                double desY = (cur.x - start.x) / (end.x - start.x)  * (end.y - start.y) + start.y;
                shSpeed = -(int)((end.x - cur.x) * disP);
                goSpeed = (int)((desY - cur.y) * disP / 2);
                // 调整进行限制
                Console.WriteLine(shSpeed);
                goSpeed = getLimit(goSpeed, maxGo, minGo, false, true);
                shSpeed = getLimit(shSpeed, maxSh, minSh, true, true);
                //Console.WriteLine(end.x + " " + cur.x);
                // Console.WriteLine(shSpeed);
                // 平移时屏蔽前后方向
                goSpeed = 0;
            }
            // Y方向是主要偏差 
            else {
                double desX = (cur.y - start.y) / (end.y - start.y) * (end.x - start.x) + start.x;
                goSpeed = (int)((end.y - cur.y) * disP);
                shSpeed = -(int)((desX - cur.x) * disP / 2);
                // 速度调整进行限制
                shSpeed = getLimit(shSpeed, maxSh, minSh, false, true);
                goSpeed = getLimit(goSpeed, maxGo, minGo, true, true);

                Console.WriteLine(shSpeed);
            }
            Console.WriteLine("Head " + goSpeed);
        }
        /// <summary>
        /// 车头朝后时，AGV控制策略
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="cur"></param>
        /// <param name="goSpeed"></param>
        /// <param name="shSpeed"></param>
        public static void faceBack(KeyPoint start, KeyPoint end, KeyPoint cur, ref int goSpeed, ref int shSpeed) {
            faceFront(start, end, cur, ref goSpeed, ref shSpeed);
            // 后向与前向区别在于速度正负问题
            goSpeed = -goSpeed;
            shSpeed = -shSpeed;
            Console.WriteLine("Back");
        }

        public static void faceRight(KeyPoint start, KeyPoint end, KeyPoint cur, ref int goSpeed, ref int shSpeed) {
            // X方向为主要偏差
            if (Math.Abs(start.y - end.y) < Math.Abs(start.x - end.x)) {
                double desY = (cur.x - start.x) / (end.x - start.x) * (cur.y - start.y) + start.y;
                goSpeed = (int)((end.x - cur.x) * disP);
                shSpeed = (int)((desY - cur.y) * disP);
                // 调整进行限制
                goSpeed = getLimit(goSpeed, maxGo, minGo, true, true);
                shSpeed = getLimit(shSpeed, maxSh, minSh, false, true);
            }
            // Y方向是主要偏差 
            else {
                double desX = (cur.y - start.y) / (end.y - start.y) * (cur.x - start.x) + start.x;
                shSpeed = (int)((end.y - cur.y) * disP);
                goSpeed = (int)((desX - cur.x) * disP);
                // 速度调整进行限制
                shSpeed = getLimit(shSpeed, maxSh, minSh, true, true);
                goSpeed = getLimit(goSpeed, maxGo, minGo, false, true);
            }
            Console.WriteLine("R " + goSpeed);
        }

        public static void faceLeft(KeyPoint start, KeyPoint end, KeyPoint cur, ref int goSpeed, ref int shSpeed) {
            faceRight(start, end, cur, ref goSpeed, ref shSpeed);
            // 后向与前向区别在于速度正负问题
            goSpeed = -goSpeed;
            shSpeed = -shSpeed;
            Console.WriteLine("L");
        }

        /// <summary>
        /// 根据当前信息，计算行驶速度
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="cur"></param>
        /// <param name="originAngle"></param>
        /// <param name="goSpeed"></param>
        /// <param name="shSpeed"></param>
        public static void getDefaultSpeed(KeyPoint start, KeyPoint end, KeyPoint cur, 
            double originAngle, ref int goSpeed, ref int shSpeed) {
                
            Direction dir = getDirection(originAngle);
            if (dir == Direction.Front) {
                faceFront(start, end, cur, ref goSpeed, ref shSpeed);
            }
            else if (dir == Direction.Back) {
                faceBack(start, end, cur, ref goSpeed, ref shSpeed);
            }
            else if (dir == Direction.Left) {
                faceLeft(start, end, cur, ref goSpeed, ref shSpeed);
            }
            else {
                faceRight(start, end, cur, ref goSpeed, ref shSpeed);
            }
        }

        /// <summary>
        /// 计算旋转速度
        /// </summary>
        /// <param name="des"></param>
        /// <param name="cur"></param>
        /// <param name="rotSpeed"></param>
        public static void getRotSpeed(KeyPoint des, KeyPoint cur, ref int rotSpeed) {
            // 比例控制速度
            int curRot = (int)(angP * (des.w - cur.w));
            // 限制速度在合理范围
            rotSpeed = Navigation.getLimit(curRot, maxRo, minRo, true, true);
        }

        /// <summary>
        /// 更新速度限制参数
        /// </summary>
        public static void updateLimit() {
            try {
                maxGo = InfoManager.moveIF.MaxFront;//int.Parse(DataArea.infoModel.Data[(int)FileInfo.paramE.MaxFront]);
                maxSh = InfoManager.moveIF.MaxSide;//int.Parse(DataArea.infoModel.Data[(int)FileInfo.paramE.MaxSide]);
                maxRo = InfoManager.moveIF.MaxRot;//int.Parse(DataArea.infoModel.Data[(int)FileInfo.paramE.MaxRotate]);
            }
            catch (Exception) {

            }
        }

    }
}
