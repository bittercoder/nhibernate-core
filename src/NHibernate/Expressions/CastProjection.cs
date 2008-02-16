namespace NHibernate.Expressions
{
	using System;
	using Engine;
	using SqlCommand;
	using SqlTypes;
	using Type;

	/// <summary>
	/// Casting a value from one type to another, at the database
	/// level
	/// </summary>
	[Serializable]
	public class CastProjection : SimpleProjection 
	{
		private readonly IType type;
		private readonly IProjection projection;

		public CastProjection(IType type, IProjection projection)
		{
			this.type = type;
			this.projection = projection;
		}

		public override SqlString ToSqlString(ICriteria criteria, int position, ICriteriaQuery criteriaQuery)
		{
			ISessionFactoryImplementor factory = criteriaQuery.Factory;
			SqlType[] sqlTypeCodes = type.SqlTypes(factory);
			if (sqlTypeCodes.Length != 1)
			{
				throw new QueryException("invalid Hibernate type for CastProjection");
			}
			string sqlType = factory.Dialect.GetCastTypeName(sqlTypeCodes[0]);
			int loc = position*GetHashCode();
			SqlString val = projection.ToSqlString(criteria, loc, criteriaQuery);
			val = RemoveAliasesFromSql(val);

			return new SqlStringBuilder()
				.Add("cast( ")
				.Add(val)
				.Add(" as ")
				.Add(sqlType)
				.Add(")")
				.Add(" as ")
				.Add(GetColumnAliases(position)[0])
				.ToSqlString();
		}

		private static SqlString RemoveAliasesFromSql(SqlString sql)
		{
			return sql.Substring(0, sql.LastIndexOfCaseInsensitive(" as "));
		}

		public override IType[] GetTypes(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			return new IType[]{ type };
		}

		public override NHibernate.Engine.TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			return projection.GetTypedValues(criteria, criteriaQuery);
		}
	}
}