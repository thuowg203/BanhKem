import 'package:ban_banh/services/api_service.dart';
import 'package:ban_banh/view/home_view.dart';
import 'package:ban_banh/view/payment_fail_view.dart';
import 'package:ban_banh/view/payment_success_view.dart';
import 'package:ban_banh/view/signup_view.dart';
import 'package:ban_banh/view/user_profile_view.dart';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'view/splash_view.dart';
import 'view/welcome_view.dart';
import 'view/login_view.dart';

final GlobalKey<NavigatorState> navigatorKey = GlobalKey<NavigatorState>();

void main() {
  runApp(const CakeApp());

}

/// Hàm lắng nghe deep link


class CakeApp extends StatelessWidget {
  const CakeApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Cake Monkey',
      debugShowCheckedModeBanner: false,
      navigatorKey: navigatorKey,
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [
        Locale('vi', 'VN'),
        Locale('en', 'US'),
      ],
      theme: ThemeData(
        primaryColor: const Color(0xFFF76C2E),
        textTheme: GoogleFonts.poppinsTextTheme(),
      ),
      initialRoute: '/',
      routes: {
        '/': (context) => const SplashView(),
        '/home_view': (context) => HomeView(
          fullName: ApiService.fullName ?? "Người dùng",
          userId: ApiService.userId ?? "",
        ),


        '/welcome': (context) => const WelcomeView(),
        '/login': (context) => const LoginView(),
        '/signup': (context) => const SignupView(),

        '/payment_success': (context) => const PaymentSuccessView(),
        '/payment_fail': (context) => const PaymentFailView(),
      },
    );
  }
}
