
using Microsoft.AspNetCore.Authorization;

namespace SnowFall.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AllowAnonymousAttritue : Attribute, IAllowAnonymous
{
}

