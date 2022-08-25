using System;
using Fractural;
using WAT;

namespace Tests
{
	public class Vector2IntTests : WAT.Test
	{
		[Test]
		public void WhenAddTwoVector2Ints_ShouldGetRightAnswer()
		{
			(Vector2Int first, Vector2Int second, Vector2Int expected)[] testParams = {
				(new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0)),
				(new Vector2Int(1, 5), new Vector2Int(15, 7), new Vector2Int(16, 12)),
				(new Vector2Int(7, 8), new Vector2Int(-4, -10), new Vector2Int(3, -2))
			};

			foreach (var testParam in testParams)
			{
				Vector2Int result = testParam.first + testParam.second;
				Assert.IsEqual(result, testParam.expected, String.Format("{0} + {1} = {2}", testParam.first, testParam.second, testParam.expected));
			}
		}

		[Test]
		public void WhenMultiplyVector2Int_WithIntScalar_ShouldGetRightAnswer()
		{
			(Vector2Int vector, int scalar, Vector2Int expected)[] testParams = {
				(new Vector2Int(0, 0), 5, new Vector2Int(0, 0)),
				(new Vector2Int(1, 5), 3, new Vector2Int(3, 15)),
				(new Vector2Int(7, 8), 0, new Vector2Int(0, 0))
			};

			foreach (var testParam in testParams)
			{
				Vector2Int result = testParam.vector * testParam.scalar;
				Assert.IsEqual(result, testParam.expected, String.Format("{0} * {1} = {2}", testParam.vector, testParam.scalar, testParam.expected));
			}
		}

		[Test]
		public void WhenMultiplyVector2Int_WithFloatScalar_ShouldGetRoundedAnswer()
		{
			(Vector2Int vector, float scalar, Vector2Int expected)[] testParams = {
				(new Vector2Int(0, 0), 2.3f, new Vector2Int(0, 0)),
				(new Vector2Int(1, 5), 2.6f, new Vector2Int(3, 13)),
				(new Vector2Int(7, 8), 0, new Vector2Int(0, 0))
			};

			foreach (var testParam in testParams)
			{
				Vector2Int result = testParam.vector * testParam.scalar;
				Assert.IsEqual(result, testParam.expected, String.Format("{0} * {1}f = {2}", testParam.vector, testParam.scalar, testParam.expected));
			}
		}


		[Test]
		public void WhenDivideVector2Int_WithIntScalar_ShouldGetRightAnswer()
		{
			(Vector2Int vector, int scalar, Vector2Int expected)[] testParams = {
				(new Vector2Int(0, 0), 5, new Vector2Int(0, 0)),
				(new Vector2Int(5, 8), 2, new Vector2Int(2, 4)),
				(new Vector2Int(7, 8), 3, new Vector2Int(2, 2))
			};

			foreach (var testParam in testParams)
			{
				Vector2Int result = testParam.vector / testParam.scalar;
				Assert.IsEqual(result, testParam.expected, String.Format("{0} / {1} = {2}", testParam.vector, testParam.scalar, testParam.expected));
			}
		}

		[Test]
		public void WhenDivideVector2Int_WithFloatScalar_ShouldGetRoundedAnswer()
		{
			(Vector2Int vector, float scalar, Vector2Int expected)[] testParams = {
				(new Vector2Int(0, 0), 2.33f, new Vector2Int(0, 0)),
				(new Vector2Int(5, 8), 2.5f, new Vector2Int(2, 3)),
				(new Vector2Int(7, 8), 3.6f, new Vector2Int(2, 2))
			};

			foreach (var testParam in testParams)
			{
				Vector2Int result = testParam.vector / testParam.scalar;
				Assert.IsEqual(result, testParam.expected, String.Format("{0} / {1}f = {2}", testParam.vector, testParam.scalar, testParam.expected));
			}
		}

		[Test]
		public void WhenNegateVector2Int()
		{
			(Vector2Int vector, Vector2Int expected)[] testParams = {
				(new Vector2Int(0, 0), new Vector2Int(0, 0)),
				(new Vector2Int(5, 8), new Vector2Int(-5, -8)),
				(new Vector2Int(-7, 8), new Vector2Int(7, -8))
			};

			foreach (var testParam in testParams)
			{
				Vector2Int result = -testParam.vector;
				Assert.IsEqual(result, testParam.expected, String.Format("-{0} = {1}", testParam.vector, testParam.expected));
			}
		}

		[Test]
		public void WhenDivideVector2IntByZero_ShouldThrowError()
		{
			Vector2Int one = new Vector2Int(3, 5);
			Vector2Int result;
			Assert.Throws(() =>
			{
				result = one / 0;
			}, "Expected exception with integer 0.");
			Assert.Throws(() =>
			{
				result = one / 0f;
			}, "Expected exception with float 0.");
		}
	}
}