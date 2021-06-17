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

contributer : ([@MoTheGoat](https://github.com/mothegoat)) </br>
              ([@AlirezaEiji191379](https://github.com/AlirezaEiji191379))

telegram : @PishroRezaEiji79  @HolyMHD </br>

<h2> how to use Api? </h2>
each part has example for how to interact with Api!

<h3> sign up </h3>
<b>Http Post </b> </br> http://localhost:5000/User/SignUp    </br> 
data =>json format! </br>
{"Username":"alirezaeiji151379","Password":"123456","Email":"alirezaeiji151379@gmail.com","PhoneNumber":"09194165232","Firstname":"alireza","Lastname":"ایجی"} </br>
</br>
it will sends confirmation code to the email that you mentioned in the data! for verification you must send the code to the api! </br>

<h3> verifing sign up </h3>

<b>Http Get</b> </br>  http://localhost:5000/Verify/VerifySignUp/{username}/{code} </br>
<ol>
<li> if you try verification code more than 5 times with failure you must restart the sign up operation! </li>
<li> after 15 min the verification code expires!</li>
</ol>

<h3> Resending Verification Code </h3>
<b>Http Get</b>  </br> http://localhost:5000/Verify/ResendCodeSignUp/{username} </br>
Resend another verification code to your email!
<b>you only can resend verification code once!</b>

<h3> siging in to your account </h3>
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

<h3>resending sign in code</h3>
<b>you can request another login code just for once! </b> </br>
<b>Http Get</b> http://localhost:5000/Verify/ResendCodeLogin/{username}


<h3>verifing signIn Code</h3>
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









  
  





