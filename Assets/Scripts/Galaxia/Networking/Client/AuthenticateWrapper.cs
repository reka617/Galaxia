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
            //���� ���϶� ���
            Debug.LogWarning("�̹� ���� ���Դϴ�.");
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

        //do auth ����� �̵�
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
                //unityservices�� �ʱ�ȭ ���� ���� ���   
                Debug.LogError(e);
                AuthState = AuthState.Failed;
            }

            count++;
            await Task.Delay(1000);
        }

        //��� �õ��� ������ ��� ó�� 
        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"�÷��̾� ���� ����: {count} �� �õ�.");
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


