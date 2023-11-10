# loginPT
Este es el proyecto completo que cuenta con las dos soluciones:
- Apiuser: solución ASP.NET Core Web API subida a Azure, cuenta con auth basic para las peticiones. Esta conectada a una BBDD alojada en AWS que es donde estan metidos los datos de prueba. La API se aloja en Azure.
- Webuser: solución ASP.NET Core (MVC), cuenta con autenticación mediante token JWT recuperados de la API, esta esta alojada en Azure y su Url es https://webuser2023.azurewebsites.net/
- Datos de prueba: Correo: usuario0@midwaytest.tech
                   Contraseña: Todo0
  hay hasta 7 usuarios de prueba con la misma estructura de credencial solo cambiando 0 por 1,2,3,4... en correo y contraseña.
