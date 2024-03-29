// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.7.7.5
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace playground.kafka
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using global::Avro;
	using global::Avro.Specific;
	
	public partial class Pessoa : ISpecificRecord
	{
		public static Schema _SCHEMA = Schema.Parse("{\"type\":\"record\",\"name\":\"Pessoa\",\"namespace\":\"playground.kafka\",\"fields\":[{\"name\"" +
				":\"id\",\"type\":\"string\"},{\"name\":\"nome\",\"type\":\"string\"}]}");
		private string _id;
		private string _nome;
		public virtual Schema Schema
		{
			get
			{
				return Pessoa._SCHEMA;
			}
		}
		public string id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		public string nome
		{
			get
			{
				return this._nome;
			}
			set
			{
				this._nome = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.id;
			case 1: return this.nome;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.id = (System.String)fieldValue; break;
			case 1: this.nome = (System.String)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
