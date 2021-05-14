using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation2D
{
	public Simulation2D(float h = 1f)
	{
		t_ = 0;
		particles_ = new LinkedList<Particle2D>();
		h_ = h;
		UpdateKernelFactors(h_);
	}

	public Simulation2D(string state, float h = 1f) : this(h)
	{
		Import(state);
	}

	public void NextFrame(float frametime)
	{
		Debug.Log("Rendering new frame: time is currently " + t_);
		float goal = t_ + frametime;
		while(t_ < goal)
		{
			NextStep(Mathf.Min(max_timestep_, goal - t_));
		}
	}

	// Simulate the next simulation step (non-constant length)
	public void NextStep(float max_steptime)
	{

		// density calculation pass
		foreach (Particle2D p in particles_)
		{
			float density = 0.000001f; // prevent div by 0 
			// for each particle, get neighbors and calculate density
			// summation approximates integration for the discrete particles
			foreach (Particle2D q in particles_)
			{
				float r = (p.pos_ - q.pos_).magnitude;
				if (p != q && r < h_)
				{
					float w = KernelPoly6(r, h_);
					density += q.m_ * w;
					//Debug.Log("Density increased by " + q.m_ * w + " based on kernel output " + w + " and particle positions p, q " + p.pos_ + " " + q.pos_ + " w/ distance " + (p.pos_-q.pos_).magnitude);
				}
			}
			p.d_ = density;
			p.p_ = (p.d_ - density_of_water_);
		}

		// force calculation pass
		foreach(Particle2D p in particles_)
		{
			Vector2 force = new Vector2(0, 0);
			foreach (Particle2D q in particles_)
			{
				float r = (p.pos_ - q.pos_).magnitude;
				if (p != q && r < h_)
				{
					// pressure between particles
					float pressure = k_ * q.m_ * (p.p_ + q.p_) / (2f * q.d_) * KernelSpiky(r, h_);
					//Debug.Log("Pressure term of " + pressure + " based on kernel function output " + KernelSpiky(r, h_));
					//Debug.Log("\tp.p " + p.p_ + " p.d " + p.d_ + " q.p " + q.p_ + " q.d " + q.d_ + " q.m " + q.m_);
					force += (p.pos_ - q.pos_).normalized * pressure;

					// viscosity between particles
					float viscosity = -u_ * (q.m_ / q.p_) * KernelViscosity(r, h_);
					force += viscosity * (q.v_ - p.v_);
					//Debug.Log("Viscosity term of " + viscosity);

					// TODO external forces (user input)

				}
				// gravitational force
				force += new Vector2(0, -g_) * p.m_;
			}
			p.f_ = force;
		}

		// Calculate timestep
		float max_v = 0;
		foreach (Particle2D p in particles_)
		{
			max_v = Mathf.Max(p.v_.magnitude, max_v);
		}
		float dt = Mathf.Min(max_steptime, h_ / (1.33f * max_v));

		// Euler integration, then keep particles in bounds
		foreach (Particle2D p in particles_)
		{
			Vector2 accel = p.f_ / p.m_;
			//Debug.Log("Modifying velocity by " + accel + " over a time of " + dt + " seconds");
			p.v_ = p.v_ + (accel * dt);
			p.pos_ = p.pos_ + (p.v_ * dt);

			if(p.pos_.x < 0 || p.pos_.x > width_)
			{
				p.pos_.x = Mathf.Min(Mathf.Max(p.pos_.x, 0), width_);
				p.v_.x = p.v_.x / 2;
			}
			if (p.pos_.y < 0 || p.pos_.y > height_)
			{
				p.pos_.y = Mathf.Min(Mathf.Max(p.pos_.y, 0), height_);
				p.v_.y = p.v_.y / 2;
			}
		}

		t_ += dt;
	}

	// Very basic simulation rendering to a texture
	public Texture2D Render(int render_width, int render_height)
	{
		// TODO letterbox instead of stretch aspect ratio?
		Color[] pixels = new Color[render_width * render_height];
		foreach(Particle2D p in particles_)
		{
			int x = (int)(p.pos_.x * (render_width - 1) / width_);
			int y = (int)(p.pos_.y * (render_height - 1) / height_);
			//Debug.Log(">>Set pixel " + new Vector2(x, y));
			pixels[x + y * render_height] = p.color_;
		}
		Texture2D texture = new Texture2D(render_width, render_height);
		texture.SetPixels(pixels);
		texture.Apply();
		return texture;
	}

	public void AddParticle(Particle2D p)
	{
		particles_.AddLast(p);
	}

	// Import simulation state - consider special Particle2D constructor
	// TODO separate into smaller functions for readability
	public void Import(string state)
	{
		// Parse state; add particles/load scenario
		t_ = 0;
		particles_.Clear();
		string[] tokens = state.Split();
		int i = 0;
		while(i < tokens.Length)
		{
			switch (tokens[i])
			{
				// TODO check for particles with missing/invalid position, velocity, mass, color
				// also check for other forms of bad input
				case "NEW_P":
					int j = i + 1;
					bool ok = true;
					Particle2D particle = new Particle2D();
					while (ok)
					{
						switch (tokens[j])
						{
							case "POS":
								particle.pos_ = new Vector2(float.Parse(tokens[j + 1]), float.Parse(tokens[j + 2]));
								j++;
								break;
							case "V":
								particle.v_ = new Vector2(float.Parse(tokens[j + 1]), float.Parse(tokens[j + 2]));
								j++;
								break;
							case "M":
								particle.m_ = float.Parse(tokens[j + 1]);
								j++;
								break;
							case "D":
								particle.d_ = float.Parse(tokens[j + 1]);
								j++;
								break;
							case "P":
								particle.p_ = float.Parse(tokens[j + 1]);
								j++;
								break;
							case "F":
								particle.f_ = new Vector2(float.Parse(tokens[j + 1]), float.Parse(tokens[j + 2]));
								j++;
								break;
							case "RGB":
								particle.color_ = new Color(float.Parse(tokens[j + 1]), float.Parse(tokens[j + 2]), float.Parse(tokens[j + 3]));
								j++;
								break;
							case "END_P":
								ok = false;
								j++;
								break;
							default:
								j++;
								break;
						}
					}
					particles_.AddLast(particle);
					i++;
					break;
				case "TIME":
					t_ = float.Parse(tokens[i + 1]);
					i++;
					break;
				case "SMOOTHING_DISTANCE":
					h_ = float.Parse(tokens[i + 1]);
					UpdateKernelFactors(h_);
					i++;
					break;
				case "GRAVITY":
					g_ = float.Parse(tokens[i + 1]);
					i++;
					break;
				case "SCENARIO":
					//TODO additional presets
					i++;
					break;
				case "DAMBREAK":
				case "RECT_FILL":
					// demo scenario generator
					int x = int.Parse(tokens[i + 1]);
					int y = int.Parse(tokens[i + 2]);
					int w = int.Parse(tokens[i + 3]);
					int h = int.Parse(tokens[i + 4]);
					int seed = int.Parse(tokens[i + 5]);
					Random.InitState(seed);
					Color water_color = new Color(1f, 1f, 1f);
					int spacing = Mathf.Max(Mathf.RoundToInt(h_ / 2), 1);
					for (int n = 0; n < w; n += spacing)
					{
						for(int m = 0; m < h; m += spacing)
						{
							Particle2D particle_d = new Particle2D();
							particle_d.pos_ = new Vector2(x + n + Random.Range(-0.3f, 0.3f), y + m + Random.Range(-0.3f, 0.3f));
							particle_d.v_ = new Vector2(Random.Range(-1, 1f), Random.Range(-1f, 0.5f));
							particle_d.m_ = 5000;
							particle_d.color_ = water_color;
							particles_.AddLast(particle_d);
						}
					}
					i++;
					break;
				default:
					i++;
					break;
			}
		}
	}

	// TODO export simulation state - consider Particle2D.toString or osmething
	public string Export()
	{
		return "";
	}

	void UpdateKernelFactors(float h)
	{
		//poly6_factor_ = 315f / (64f * Mathf.PI * Mathf.Pow(h_, 9)); // seems to be for 3D
		poly6_factor_ = 4 / (Mathf.PI * Mathf.Pow(h, 8));
		spiky_factor_ = 15 / (Mathf.PI * Mathf.Pow(h, 4));
		viscosity_factor_ = (40 * u_) / (Mathf.PI * Mathf.Pow(h_,4));
	}

	// Poly6 kernel - density
	// Where r is distance between two particles, h is smoothing distance
	float KernelPoly6(float r, float h)
	{
		if(r < h)
			return poly6_factor_ * Mathf.Pow(Mathf.Pow(h, 2) - Mathf.Pow(r, 2), 3);
		return 0;
	}

	// Gradient of spiky kernel - pressure
	float KernelSpiky(float r, float h)
	{
		if (r < h)
		{
			float q = (r + 0.000001f) / h;
			return spiky_factor_ * r * Mathf.Pow(1 - q, 2) / q;
		}
		return 0;
	}

	// Laplacian of viscosity kernel - viscosity
	// note: Laplacian in 2D different from in 3D
	float KernelViscosity(float r, float h)
	{
		if (r < h)
			return viscosity_factor_ * (1 - r / h);
		return 0;
	}

	// Set up pgrid_ for the frame
	// Each list represents an h_ by h_ region
	// Array includes buffer regions on edges
	// Currently unused
	void RebuildPgrid()
	{
		pgrid_ = new List<Particle2D>[Mathf.CeilToInt((float)width_ / h_) + 2, Mathf.CeilToInt((float)height_ / h_) + 2];
		foreach (Particle2D p in particles_)
		{
			pgrid_[Mathf.FloorToInt(p.pos_.x / h_) + 1, Mathf.FloorToInt(p.pos_.y / h_) + 1].Add(p);
		}
	}

	LinkedList<Particle2D> particles_;
	List<Particle2D>[,] pgrid_; // spatial division optimization; not a MAC grid
	float t_; // time
	float h_; // smoothing width
	float poly6_factor_; // kernel normalization factors?
	float spiky_factor_;
	float viscosity_factor_;
	float g_; // gravity
	float width_ = 512, height_ = 512;
	float k_ = 1000; // fluid "stiffness"? / specific gas constant-ish value
	float u_ = 1000; // viscosity related constant
	float density_of_water_ = 1000; // approx density of water, kg/m3
	float max_timestep_ = 0.01f; // max timestep, seconds
	// float density_of_air_ = 1.204f; // reference density (of atmosphere), kg/cubic meter

	// TODO user tools - add, remove particles/walls/...
	// TODO definable fluid properties -> mixing separate fluid types at once
	// TODO acceleration struct - divide space to cells w/ size related to h, each frame rebuild lists based on particle positions

	//float atm_pressure_ = 1f; // reference pressure (of atmosphere), in atmospheres (1 atm = 101325 Pascals)
	//float gamma_ = 7f; // heat capacity ratio / adiabatic index
	//public static readonly float PI_POW_THREEHALVES = Mathf.Pow(Mathf.PI, 1.5f);
}
