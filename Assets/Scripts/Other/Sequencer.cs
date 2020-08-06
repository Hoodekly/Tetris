using System.Collections.Generic;

public class Sequencer
{
	public delegate void Step(float progress);

	private Step[] steps;
	private float[] stepsDuration;

	public void SetSteps(params Step[] steps)
	{
		this.steps = steps;
	}
	public void SetStepsDuration(params float[] stepsDuration)
	{
		this.stepsDuration = stepsDuration;
	}
	public void Invoke(float elapsedTime)
	{
		int stepId = steps.Length - 1;
		float curProgress = 0f;
		for (int i = 0; i < stepsDuration.Length; i++)
		{
			if (elapsedTime < stepsDuration[i])
			{
				curProgress = elapsedTime / stepsDuration[i];
				stepId = i;
				break;
			}
			elapsedTime -= stepsDuration[i];
		}
		steps[stepId](curProgress);
	}
}
