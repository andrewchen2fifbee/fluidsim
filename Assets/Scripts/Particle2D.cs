using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D
{
	public Particle2D()
	{
		
	}
	public Particle2D(Vector2 pos, Vector2 v, float m, float d, float p, Vector2 f, Color color)
	{
		pos_ = pos;
		v_ = v;
		m_ = m;
		d_ = d;
		p_ = p;
		f_ = f;
		color_ = color;
	}

	// make everything public because lazy
	public Vector2 pos_;
	public Vector2 v_; // velocity
	public float m_; // mass
	public float d_; // density
	public float p_; // pressure
	public Vector2 f_; // force applied
	public Color color_;
}
