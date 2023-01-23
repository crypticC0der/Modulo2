using UnityEngine;

namespace Modulo{
	public static class Effects{
		static int initVel=7;
		public static void Explode(Vector3 center,float strength,Damager sender,int mask=-1,float radius=-1){
			//generate particles
			float size= Mathf.Max((strength)*1.5f,0.6f);
			if(radius!=-1){
				size=radius;
			}
			Collider2D[] c = Physics2D.OverlapCircleAll(center,size,mask);
			foreach(Collider2D col in c){
				Damageable D = col.GetComponent<Damageable>();
				if(D){
					D.ApplyDebuff<Burning>(null,(int)(1*strength));
					DamageData d = new DamageData{dmg=10*strength,direction=D.transform.position - center,properties=DamageProperties.bypassArmor};
					if(sender==null){
						D.TakeDamage(d);
					}else{
						sender.DmgOverhead(d,D);
					}
				}
			}

			GameObject particleSim = new GameObject("particleSim");
			ParticleSystem ps = particleSim.AddComponent<ParticleSystem>();

			ParticleSystem.MainModule main = ps.main;
			ps.Stop();
			main.duration=0.5f;
			main.startLifetime=Mathf.Max((1f/initVel) * (size),0.2f);
			main.startSpeed=size/(main.startLifetime.constant);
			main.loop=false;
			main.startSize=0.8f;
			main.startSize3D=false;
			main.simulationSpace=ParticleSystemSimulationSpace.Local;

			ParticleSystem.SizeOverLifetimeModule sizemod = ps.sizeOverLifetime;
			sizemod.enabled=true;

			ParticleSystem.MinMaxCurve curve = main.startSize;
			curve.mode = ParticleSystemCurveMode.Curve;
			curve.curve = new AnimationCurve(new Keyframe(0,0.8f,-0.8f,-0.8f),new Keyframe(1,0.8f/10,-0.8f,-0.8f));
			sizemod.size=curve;


			ParticleSystem.EmissionModule em = ps.emission;
			em.rateOverTime=0;
			ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[1];
			bursts[0].count=25*size*size;
			bursts[0].time=0;
			em.SetBursts(bursts);

			ParticleSystem.ShapeModule shape = ps.shape;
			shape.shapeType=ParticleSystemShapeType.Sphere;

			ParticleSystem.ColorOverLifetimeModule colour = ps.colorOverLifetime;
			colour.enabled=true;
			colour.color= new ParticleSystem.MinMaxGradient(Effects.BurnGradient());

			ParticleSystemRenderer pr = particleSim.GetComponent<ParticleSystemRenderer>();
			pr.material=Resources.Load<Material>("particle material");
			particleSim.transform.position=center;
			ps.Play();
		}

		public static Gradient BurnGradient(){
			GradientColorKey[] colorKeys=new GradientColorKey[3];
			colorKeys[0].color=MeshGen.MeshGens.ColorFromHex(0xffb400ff);
			colorKeys[0].time=0f;
			colorKeys[1].color=MeshGen.MeshGens.ColorFromHex(0x842610ff);
			colorKeys[1].time=0.5f;
			colorKeys[2].color=MeshGen.MeshGens.ColorFromHex(0x260900ff);
			colorKeys[2].time=1f;
			// colorKeys[3].color=MeshGen.MeshGens.ColorFromHex(0x000000ff);
			// colorKeys[3].time=1f;
			GradientAlphaKey[] alphaKeys= new GradientAlphaKey[2];
			alphaKeys[0].alpha=1;
			alphaKeys[0].time=0;
			alphaKeys[1].alpha=1;
			alphaKeys[1].time=1;
			Gradient g= new Gradient();
			g.SetKeys(colorKeys,alphaKeys);
			return g;
		}
	}
}
