using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public enum AuthState
{
   NotAuthenticated,
   Authenticating,
   Authenticated,
   Failed,
   Timeout
}
public static class AuthenticateWrapper
{
   public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

   public static async Task<AuthState> DoAuth(int triedCount = 5)
   {
      //인증이 되어있으면 권한부여 할 필요 없음
      if (AuthState == AuthState.Authenticated) return AuthState;
      
      if (AuthState == AuthState.Authenticating)
      {
         //인증 중일 떄
         Debug.LogError("Already being authenticated");
         await Authenticating();
         return AuthState;
      }
      
      //AuthenticationService는 UGS와 통신하기 위한 것
      await SignInAnonymousAsync(triedCount);
      
      return AuthState;
   }

   private static async Task<AuthState> Authenticating()
   {
      while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
      {
         await Task.Delay(300);
      }

      return AuthState;
   }

   private static async Task SignInAnonymousAsync(int triedCount)
   {
      //DoAuth의 기능 이동
      AuthState = AuthState.Authenticating;
      int count = 0;

      while (AuthState == AuthState.Authenticating && count < triedCount)
      {
         try
         {
            //AutenticatiobService는 UnityService와 연결하기 위함
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
               AuthState = AuthState.Authenticated;
               
               break;
            }

         }
         catch (AuthenticationException e)
         {
            Debug.LogError(e);
            AuthState = AuthState.Failed;
         }
         catch (RequestFailedException e)
         {
            //UnityServices가 초기화 되지 않은 경우
            Debug.LogError(e);
            AuthState = AuthState.Failed;
         }

         count++;
         await Task.Delay(1000);
      }

      if (AuthState != AuthState.Authenticated)
      {
         Debug.LogWarning($"Player Authentication failed.. TryCount : {count} ");
         AuthState = AuthState.Timeout;
      }
   }
}
