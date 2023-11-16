
using MediatR;
using SnowFall.Attributes;
using SnowFall.Exceptions;
using SnowFall.Interfaces;
using System.Reflection;

namespace SnowFall.Behaviours;


public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    readonly IAuthUser _authUser;

    public AuthorizationBehaviour(IAuthUser authUser)
    {
        _authUser = authUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        IEnumerable<AuthorizeAttribute> authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            if (_authUser == null)
                throw new AuthorizationException();

            if (!_authUser.isAuthenticated)
                throw new AuthorizationException("로그인 후 이용 가능합니다.");

            //if (string.IsNullOrEmpty(_loginUser.Pid))
            //    throw new AuthorizationException("인증정보가 유효하지 않습니다.");

            //인증정보의 문화권으로 현재 문화권을 설정
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_loginUser.CultureName);

            // 추가 인증/권한 을 체크해야할 경우
            // 권한등(ex. 카페장만 접근가능한 Command나 Query) 처리
            //foreach (var authorize in authorizeAttributes)
            //{
            //    if(authorize.UserRole == _loginUser.UserRole)
            //        return await next();
            //}
            //throw new AuthorizationException("접근 권한이 없습니다.");
        }

        return await next();
    }
}