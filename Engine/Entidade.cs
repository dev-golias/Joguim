﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; 
using static Engine.Helper;
using Microsoft.Xna.Framework.Input;

namespace Engine
{

	public abstract class Entidade : IEntidade{

        public virtual Estilo Estilo { get; set; }
		public virtual Cord Pos { get ; set; }
		public virtual Tamanho Tam { get ; set; }
        public virtual PosicaoLados Lados => new PosicaoLados(Pos, Tam);
        public virtual void Atualizar(double DeltaTempo){}
        public Entidade(Cord posicao, double largura = TamanhoPadrao, double altura= TamanhoPadrao, Estilo estilo =  default )
        {
            Pos = posicao;
            Tam = new Tamanho(this, largura, altura);
            Estilo = estilo;
        }
    }
    public class Jogador : Entidade, IInputable, IMovel, IColisivel
	{
		public Inputs Inputs {get;init;}
        public Movimento Mov {get;init;}
		public Jogador(Cord posicao, double largura = TamanhoPadrao, double altura = TamanhoPadrao, Estilo estilo = default) : base(posicao, largura, altura, estilo)
		{
            Mov =  new Movimento(this, Vetor.Zero);
            Inputs =  new Inputs();
        }
        public void Colidir(IEntidade colisor){
            Colisao.Movel(this,colisor);
        }
        public Vetor VelocidadeDirecionais;

		public override void Atualizar(double DeltaTempo)
		{
            Mov.Atualizar(DeltaTempo);
            throw new NotImplementedException();
		}
	}
    [Serializable]
    public class Parede : Entidade, IColisivel
    {

        public void Colidir(IEntidade Colisor) => Colisao.Estatica(this, Colisor);
        public Parede(Cord posicao, double largura = TamanhoPadrao, double altura= TamanhoPadrao, Estilo? estilo = default) 
        : base (posicao, largura, altura, estilo ?? Estilo.parede)
        {
        }
         
    }
    [Serializable]
    public class Morte : Entidade, IColisivel 
    {   
        public void Colidir(IEntidade Colisor)
        {
            if (Colisor is IJogador) 
                ((IJogador)Colisor).Dano(1);
             Colisao.Estatica(this, Colisor);
        }
        public Morte(Cord posicao, double largura = TamanhoPadrao, double altura = TamanhoPadrao, Estilo? estilo = null)  
            : base (posicao, largura,altura, estilo ?? Estilo.Morte)
        {  
        }
    }
    [Serializable]
    public class Teletransporte : Entidade, IColisivel
    { 
        public Cord Saida; 
        public Teletransporte(Cord posicao, Cord saida , Estilo? estilo = null, double largura = TamanhoPadrao, double altura = TamanhoPadrao) 
            : base (posicao, largura,altura, estilo ?? Estilo.Teletransporte)
        { 
            Saida = saida;
        }
        public void Colidir(IEntidade Colisor)
        { 
            if (Colisor is IJogador jogador) 
                jogador.Mover(Saida);
            Colisao.Estatica(this, Colisor);
        }
    }
    [Serializable]
    public class Porta : Entidade, IColisivel, IReceptor
    {  
        private Estilo EstiloFechada { get; set; }
        private Estilo EstiloAberta { get; set; } 
        public bool Aberta => RequesitosCompletos >= ResquisitosNescessarios;
        private int ResquisitosNescessarios { get; set; }
        private int RequesitosCompletos { get; set; }
        public void Receber(object e)
        {
            RequesitosCompletos++;
        }

        public void Colidir(IEntidade Colisor) {
            if(Aberta)             
                Colisao.Estatica(this, Colisor);

        } 
        

        public Porta(Cord posicao, int resquisitosNescessarios, Estilo? estiloFechada = null, Estilo? estiloAberta = null, double largura = TamanhoPadrao, double altura = TamanhoPadrao) 
            :base(posicao, altura, largura, Estilo.parede)
        {
            EstiloFechada = estiloFechada ?? new Estilo(Color.SaddleBrown);
            EstiloAberta = estiloAberta ?? new Estilo(Color.Sienna); ;
            ResquisitosNescessarios = resquisitosNescessarios;
        }
    }
    [Serializable]
    public class Botao :Entidade, IColisivel
    { 
		public bool Prescionado = false;
        public IReceptor Receptor;
        public void Emitir()
        {
            Receptor.Receber(this);
        }

        public void Colidir(IEntidade e = null)
        {
            if (!Prescionado)
            {
                Emitir();
                Prescionado = true;
            } 
        }

		public void Atualizar(long DeltaTempo){}

		public Botao(Cord posicao, IReceptor receptor, double largura = TamanhoPadrao, double altura = TamanhoPadrao, Estilo? estilo = null) 
        : base(posicao, largura, altura, estilo ?? Estilo.Botao)
        {
            Receptor = receptor; 
        }


    }
    [Serializable]
    public class Quadradinho :  Entidade, IMovel, IColisivel
    {
        public Movimento Mov {get;}  
        public bool HorarioAntihoriario;
        public void Colidir(IEntidade Colisor)
        {
            Mov.Velocidade *= HorarioAntihoriario ^ Mov.Velocidade.x == 0 ? 1 : -1; ;
            Colisao.Estatica(this, Colisor);

        }

		public Quadradinho(Cord posicao, bool horarioAntihoriario, Vetor direcao, double largura = TamanhoPadrao, double altura = TamanhoPadrao, Estilo? estilo = null) 
         :base(posicao, largura, altura, estilo ?? Estilo.Aleatorio())
        {
            HorarioAntihoriario = horarioAntihoriario;
            Mov = new Movimento(this, direcao.Normalizar()); 
        }

    }
    [Serializable]
    public class BateVolta :Entidade,  IMovel, IColisivel
    {   
        public BateVolta(Cord posicao, Vetor?  Direcao = null, double largura=TamanhoPadrao, double altura= TamanhoPadrao, Estilo? estilo = null)
            :base(posicao,largura,altura, estilo ??  Estilo.Aleatorio())
        {
            Mov = new Movimento(this,Direcao ?? default); 
        }
		public Movimento Mov {get;set;} 
		public override void Atualizar(double DeltaTempo){
            Mov.Atualizar(DeltaTempo);
        }
		public void Colidir(IEntidade Colisor)
		{
            Colisao.Movel(this, Colisor);
		}
	}
    public class Particula : Entidade, IMovel, IColisivel
    { 
        public Movimento Mov {get;}  
        public override Cord Pos {
            get => Vivo ? base.Pos : Cord.NaN;
            set {
                if(Vivo)
                    base.Pos = value;
            }
        }
        public bool Vivo => TempoVidaMax > TempoVida ;
        public double TempoVidaMax;
        public double TempoVida => Tempo - MomentoCriacao ;
        public event Action<Particula> Morte;
        double MomentoCriacao;
        void Morrer()
        {
            Morte?.Invoke(this);
        }
        public void ZerarCriacao(){
            MomentoCriacao = Tempo;
        }
        public Particula(Cord posicao, Action<Particula> HandlerMorte = null, int? tempoVidaMax = null,Vetor? Direcao = null, Estilo? estilo = null) 
        : base (posicao, 5,5)
        {
            base.Pos = posicao;
            Morte += HandlerMorte;
            Estilo = estilo ?? new Estilo(Color.White);
            Mov = new Movimento(this, Direcao ?? new Vetor(Rnd.NextDouble() * 2 - 1, Rnd.NextDouble() * 2 - 1).Normalizar());
            TempoVidaMax = tempoVidaMax ?? Rnd.Next(0, 1000);
            ZerarCriacao();
            

        }
        public override void Atualizar( double DeltaTempo)
        { 
            if(!Vivo)
                Morrer();
            Mov.Atualizar(DeltaTempo);
        }
		public override string ToString()
		{
			return $"Pos:{Pos}";
		}
		public void Colidir(IEntidade Colisor)
		{ 
            Colisao.Movel(this, Colisor);
		}
	}
}
