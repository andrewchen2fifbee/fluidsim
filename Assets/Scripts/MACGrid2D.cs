using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Currently unused
public class MACGrid2D
{
	public MACGrid2D(int w, int h)
	{
		w_ = w;
		h_ = h;
		p_ = new float[w, h];
		u_x_ = new float[w + 1, h];
		u_y_ = new float[w, h + 1];
	}

	int w_, h_; // grid width, height
	//MACCell[][] cells;
	float[,] p_; // pressure
	float[,] u_x_; // velocities indicating movement between adjacent cells
	float[,] u_y_;
}
