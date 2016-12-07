using System;
using System.Linq;
using MathNet;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using CoreLocation;

namespace MyShop.iOS
{
	
	public class LocalizationCaculation
	{
		public static double weight = 96640;
		Dictionary<string, double> Receivedsortedbeacon;
		Dictionary<string, Store> landmarkbeacons;

		public LocalizationCaculation(Dictionary<string, double> sl, Dictionary<string, Store> landmarkbeacon)
		{
			Receivedsortedbeacon = sl;
			landmarkbeacons = landmarkbeacon;
		}

		public double caculatemeter(double rssi)
		{
			var DistanceMeter = 0.30480000000122 * (Math.Pow(10, (-rssi - 63.5379) / (10 * 2.086)) * 3);
			return DistanceMeter;
		}
		public CLLocationCoordinate2D PointLocalization()
		{
			List<Tuple<string, double>> fullid = new List<Tuple<string, double>>();
			foreach (var item in Receivedsortedbeacon.OrderByDescending(r => r.Value).Take(3))
			{
				var pair = new Tuple <string, double> (item.Key, item.Value);
				fullid.Add(pair);
				Console.WriteLine("Key: {0}, Value: {1}", item.Key, item.Value);
			}

			Point A = new Point { x = landmarkbeacons[fullid[0].Item1].Latitude, y = landmarkbeacons[fullid[0].Item1].Longitude};
			Point B = new Point { x = landmarkbeacons[fullid[1].Item1].Latitude, y = landmarkbeacons[fullid[1].Item1].Longitude };
			Point C = new Point { x = landmarkbeacons[fullid[2].Item1].Latitude, y = landmarkbeacons[fullid[2].Item1].Longitude};

			double lenAZ = caculatemeter(fullid[0].Item2)/weight;
			double lenBZ = caculatemeter(fullid[1].Item2)/weight;
			double lenCZ = caculatemeter(fullid[2].Item2)/weight;


			//先创建系数矩阵A 
			var matrixA = DenseMatrix.OfArray(new[,] { { 2 * (B.x - A.x), 2 * (B.y - A.y) }, { 2 * (C.x - A.x), 2 * (C.y - A.y) } });
			//创建向量b
			var v1 = Math.Pow(B.x, 2) - Math.Pow(A.x, 2) + Math.Pow(B.y, 2) - Math.Pow(A.y, 2) + Math.Pow(lenAZ, 2) - Math.Pow(lenBZ, 2);
			var v2 = Math.Pow(C.x, 2) - Math.Pow(A.x, 2) + Math.Pow(C.y, 2) - Math.Pow(A.y, 2) + Math.Pow(lenAZ, 2) - Math.Pow(lenCZ, 2);
			var vectorB = new DenseVector(new[] { v1, v2 });
			var resultX1 = matrixA.LU().Solve(vectorB);


			var matrixB = DenseMatrix.OfArray(new[,] { { 2 * (B.x - A.x), 2 * (B.y - A.y) }, { 2 * (C.x - B.x), 2 * (C.y - B.y) } });
			//创建向量b
			var v3 = Math.Pow(B.x, 2) - Math.Pow(A.x, 2) + Math.Pow(B.y, 2) - Math.Pow(A.y, 2) + Math.Pow(lenAZ, 2) - Math.Pow(lenBZ, 2);
			var v4 = Math.Pow(C.x, 2) - Math.Pow(B.x, 2) + Math.Pow(C.y, 2) - Math.Pow(B.y, 2) + Math.Pow(lenBZ, 2) - Math.Pow(lenCZ, 2);
			var vectorC = new DenseVector(new[] { v3, v4 });
			var resultX2 = matrixB.LU().Solve(vectorC);


			var matrixC = DenseMatrix.OfArray(new[,] { { 2 * (C.x - A.x), 2 * (C.y - A.y) }, { 2 * (C.x - B.x), 2 * (C.y - B.y) } });
			//创建向量b
			var v5 = Math.Pow(C.x, 2) - Math.Pow(A.x, 2) + Math.Pow(C.y, 2) - Math.Pow(A.y, 2) + Math.Pow(lenAZ, 2) - Math.Pow(lenCZ, 2);
			var v6 = Math.Pow(C.x, 2) - Math.Pow(B.x, 2) + Math.Pow(C.y, 2) - Math.Pow(B.y, 2) + Math.Pow(lenBZ, 2) - Math.Pow(lenCZ, 2);
			var vectorD = new DenseVector(new[] { v5, v6 });
			var resultX3 = matrixC.LU().Solve(vectorD);

			Point P = new Point {
				x = (resultX1[0] + resultX2[0] + resultX3[0]) / 3,
				y = (resultX1[1] + resultX2[1] + resultX3[1]) / 3
			};


			//Console.WriteLine("!result: " + P.x + " - " + P.y);
			//Console.WriteLine("!resultx2: "+ resultX2.ToString());
			//Console.WriteLine("!resultx3: " +resultX3.ToString());
			CLLocationCoordinate2D userlocation;
			userlocation.Latitude = P.x;
			userlocation.Longitude = P.y;

			Console.WriteLine("userlocation: " + P.x + "-" + P.y);
			return userlocation;

		}
	}
}
