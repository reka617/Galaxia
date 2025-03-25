using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticateWrapper
{   
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int triedCount = 5)
    {
        if(AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }
        
        if(AuthState == AuthState.Authenticating)
        {
            //인증 중일때 대기
            Debug.LogWarning("이미 인증 중입니다.");
            await Authenticating();
            return AuthState;
        }

        await SignInAnonymousAsync(triedCount);

        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while(AuthState == AuthState.Authenticating || AuthState== AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
            
        }

        return AuthState;
    }


    private static async Task SignInAnonymousAsync(int  triedCount)
    {

        //do auth 기능으 이동
        AuthState = AuthState.Authenticating;
        int count = 0;

        while (AuthState == AuthState.Authenticating && count < triedCount)
        {

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                //await AuthenticationService.Instance.SignInWith
                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    //Debug.Log("Auth Success");
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch(AuthenticationException e)
            {
                Debug.LogError(e);
                AuthState = AuthState.Failed;
            }
            catch(RequestFailedException e)
            {
                //unityservices가 초기화 되지 않은 경우   
                Debug.LogError(e);
                AuthState = AuthState.Failed;
            }

            count++;
            await Task.Delay(1000);
        }

        //모든 시도가 실패한 경우 처리 
        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"플레이어 인증 실패: {count} 번 시도.");
            AuthState = AuthState.Timeout;
        }
    }



}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Failed,
    Timeout
}


