# web_api_base
web api base for Authentication and Authorization with Asp.net Core web Api!</br>

<h1> Goals! </h1>
it has Jwt(which is signed token, jws) Access token for 15 min expiration time! </br>
guid refresh token for 1 week! </br>
it hashes and saltes the password and take some time for avoiding brute force attack! </br>
it supports 2fa for login and register system with gmail smtp! </br>
it sends securtiy email for unknown and suspected Ips and User-Agents! </br>
it supports RSA cryptographic System for jwt Access Token Algorithm! </br>
it has Efcore for its ORM!<br>
it supports terminating sessions like telegram and security alert emails like google!<br>

for creating the tables in database just use migration scrtipts in Visual studio!<br>

contributer :
              ([@AlirezaEiji191379](https://github.com/AlirezaEiji191379))

telegram : @PishroRezaEiji79  </br>

<h2> How to use Api? </h2>
each part has example for how to interact with Api!

<h3> Sign up </h3>
<b>Http Post </b> </br> http://localhost:5000/User/SignUp    </br> 
data =>json format! </br>
{"Username":"alirezaeiji151379","Password":"123456","Email":"alirezaeiji151379@gmail.com","PhoneNumber":"09194165232","Firstname":"alireza","Lastname":"ایجی"} </br>
</br>
it will sends confirmation code to the email that you mentioned in the data! for verification you must send the code to the api! </br>

<h3> Verifing sign up </h3>

<b>Http Get</b> </br>  http://localhost:5000/Verify/VerifySignUp/{username}/{code} </br>
<ol>
<li> if you try verification code more than 5 times with failure you must restart the sign up operation! </li>
<li> after 15 min the verification code expires!</li>
</ol>

<h3> Resending Verification Code </h3>
<b>Http Get</b>  </br> http://localhost:5000/Verify/ResendCodeSignUp/{username} </br>
Resend another verification code to your email!
<b>you only can resend verification code once!</b>

<h3> Siging in to your account </h3>
<b>Http Post</b>  
</br>http://localhost:5000/Auth/Login </br>
json data: </br>
{"Username":"alirezaeiji151379","Password":"123456"} </bt>
<ol>
  <li>if you did not verify your sign up verification code you can not login to your system!</li>
  <li>this request sends you login verification code to your email</li>
  <li>after 15 min the login verification code expires!</li>
  <li>if the username does not exist you give appopraite response!</li>
  <li>if the username was correct but the passoword was wrong the api saves your failure!</li>
  <li>after 5 failure in sign in process your account is going to be locked for 5 min and you can not sign in during that 5 min!</li>
</ol>

<h3>Resending sign in code</h3>
<b>you can request another login code just for once! </b> </br>
<b>Http Get</b> </br> http://localhost:5000/Verify/ResendCodeLogin/{username} </br>


<h3>Verifing signIn Code</h3>
after successful login process (correct username and password) for security reasons we sends you verificaion email (2FA)! </br>

<b>Http Get</b>
</br>http://localhost:5000/Verify/VerifyLogin/{username}/{code}</br>

<ol>
  <li>if you try verification with more than 5 times with failure the account is going to be locked for 5 minutes and you can not login to your account during the locked time!</li>
  <li>After successful verification the client can enter account and the api gives you two kind of token!</li>
</ol>
<b>Token format:</b> </br>
{</br>
"jwtAccessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2MjM3NTQ2MzIsImp0aSI6IjQ4NDU0MzgzLWI3YzEtNDA3Ni05YzliLWEzODY3NmI5NmU0MiIsInVpZCI6IjEyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTYyMzc1NTUzMiwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIn0.QT3h2EPNOBhj5AnMlUpEIQgfsFC9AiTXOtMfof3AyT38TSznSQvHlpPUQQ2wGkeVBT9BoKbpqq9RQv8nqz_jR3sp3ZVomrrLHfP7PAcX08lgl75XQnL1gVBVrojOU4FBdxQrGbnPjWH6ijeVHmqcEstRX_BZB9mVYI2Gu8rpSrhrpAfTmMliQ7Rq8TQnfMQASYqj7bV1EhY2GfF4eng0Hced8VWGzcDMQxHD_UbjsHCfl_K2bXmItU3Fss4loxlbiHC5IRkPnvUGkKSRofELwG8feluNEIckwuZDGIKwjl6LK-9t2zJG_CM5wgpCrkDce3YcAs2f-NEDq-cVlWzcHA", </br>
"refreshToken": "247c3a55-d7b7-4e38-8427-c5d8317905d0"</br>
} </br>

this access token is paired to the refresh token with jti which is used in the jwt access token! </br>
for decoding jwt Token you can visit <a href="http://www.jwt.io">this site </a> </br>
inside the jwt we encode Role Of user for <a href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-5.0">Role-based Authorization</a> </br>
<b>in every Authorized Request you must include the Jwt Access Token in the Authorization Bearer Header of Http request!</b> </br>

<h3>Refresh Request for Access Token</h3>

after 15 min the access token expires and for every Route which needs Authorization you would get 401 unauthorized error! so
you must refresh your access token! </br>

<b>Http Post</b> 
</br> http://localhost:5000/Refresh </br>
json data:
{</br>
"jwtAccessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2MjM3NTQ2MzIsImp0aSI6IjQ4NDU0MzgzLWI3YzEtNDA3Ni05YzliLWEzODY3NmI5NmU0MiIsInVpZCI6IjEyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImV4cCI6MTYyMzc1NTUzMiwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIn0.QT3h2EPNOBhj5AnMlUpEIQgfsFC9AiTXOtMfof3AyT38TSznSQvHlpPUQQ2wGkeVBT9BoKbpqq9RQv8nqz_jR3sp3ZVomrrLHfP7PAcX08lgl75XQnL1gVBVrojOU4FBdxQrGbnPjWH6ijeVHmqcEstRX_BZB9mVYI2Gu8rpSrhrpAfTmMliQ7Rq8TQnfMQASYqj7bV1EhY2GfF4eng0Hced8VWGzcDMQxHD_UbjsHCfl_K2bXmItU3Fss4loxlbiHC5IRkPnvUGkKSRofELwG8feluNEIckwuZDGIKwjl6LK-9t2zJG_CM5wgpCrkDce3YcAs2f-NEDq-cVlWzcHA", </br>
"refreshToken": "247c3a55-d7b7-4e38-8427-c5d8317905d0"</br>
} </br>

<ol>
  <li>if the refresh token does not exist, the request has failed response!</li>
  <li>if the refresh token was revoked, the request has failed response!</li>
  <li>if the refresh token was expired, the request has failed response!</li>
  <li>if the access token is invalid,the request has failed response!</li>
  <li>if the access token was not expired,the request has failed response!</li>  
</ol>  
if the refresh request was successful you get the new access and refresh token! </br>

<h3> logout </h3>
<b> Authorized Request </b> it means that if you want to logout you must add Authorization Bearer Header in your request! </br></br>

<b>Http Delete </b> 
</br>http://localhost:5000/Auth/logout</br>
After logging out the refresh token is revoked and the access token is blacklisted!</br>
In the Security middlerware which is Custom middleware I checked that if the token is not blacklisted! </br>

<h3> Terminate All Session </h3>
<b> Authorized Request </b> it means that if you want to logout you must add Authorization Bearer Header in your request! </br></br>
<b>Http Delete</b> </br>http://localhost:5000/Auth/TerminateAllSessions</br>

Each device has priority based on the login order to the account it means that if device A then B then C enter the account priorit of A is 1 B is 2 C is 3 and the device with lower priority can not terminate the session of the device with higher priority! for exampe if device B terminate All session the Device A is not going to be fired from account!

<h3> Terminate one Session </h3>
<b> Authorized Request </b> it means that if you want to logout you must add Authorization Bearer Header in your request! </br></br>
<b>Http Delete</b> </br>http://localhost:5000/Auth/TerminateOneSession/{priority-number}</br>

Each device has priority based on the login order to the account it means that if device A then B then C enter the account priorit of A is 1 B is 2 C is 3 and the device with lower priority can not terminate the session of the device with higher priority! for exampe if device B terminate All session the Device A is not going to be fired from account!

if the prioriy number is bigger than entered devices you get Error Response! </br>


<h3> Get All signedIn devices </h3>
<b> Authorized Request </b> it means that if you want to logout you must add Authorization Bearer Header in your request! </br></br>
<b>Http Get</b> </br>http://localhost:5000/Auth/getAllDevices</br>

this request gives you a list of User-Agent (devices) that enters an account! </br>


<h3>Change Password Request</h3>
<b>Http Post</b> </br>http://localhost:5000/User/changePassword</br>
<b> Authorized Request </b> it means that if you want to logout you must add Authorization Bearer Header in your request! </br></br>
json data: </br>
{</br>
    "oldPassword" : "UbIgaXB37Hwe",</br>
    "newPassword" : "123456"
</br>    
}

<ol>
  <li>if the user enter wrong password the request has failed response and the api sends you email that malicious client may enter your account! and you can terminate the session of that client!</li>
  <li>if the user enter correct passoword Api sends verification Code To you!</li?
</ol>

  
<h3>Verify Change Password</h3>
<b>Http Post</b> </br>http://localhost:5000/Verify/VerifyChangePassword/{username}/{code}</br>
<b> Authorized Request </b> it means that if you want to logout you must add Authorization Bearer Header in your request! </br></br>  

<ol>
  <li>After 2 failure in verification code the request has failed response!</li>
  <li>code expires after 15 min</li>
  <li>after successful verification the password changes and All the devices loged out automatically and must relogin in to the account!</li> 
</ol>
 
<h3>forget password</h3>
<b>Http Get</b> </br>http://localhost:5000/User/ForgetPassword/{email}</br>

<ol>
  <li>it sends you an email code verification for changing password</li>
  <li>the code expires after 15 min!</li>
</ol>

<h3> verify forget code passowrd </h3>
<b>Http Get</b> </br>http://localhost:5000/Verify/VerifyForgetPassword/{email}/{code}</br>

<ol>
  <li>if you try more than 2 times with failure the operation fails!</li>
  <li>the code expires after 15 min!</li>
  <li>After successful cerification we send an email includes your new password with 9 alphanumeric letters!</li>
</ol>


<h2>Security Hints</h2>

if the ip address of the token changed, we send an security alert to the email address of client that another person with that ip and user-agent is in his account and if he is malicious client who stole the token the native client can logout and relogin! </br>
if the user-agent of the token changes we revoke the related refresh token and blacklist the access token! </br>
if the access token was balcklisted the request has failed response! it was added to the SecurityMiddleware Class!











  
  





