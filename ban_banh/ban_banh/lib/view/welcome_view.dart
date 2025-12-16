import 'package:flutter/material.dart';
import '../widgets/custom_button.dart';

class WelcomeView extends StatelessWidget {
  const WelcomeView({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Expanded(
            flex: 3,
            child: Container(
              child: Center(
                child: Image.asset(
                  'assets/images/cake_logo.png',
                  height: 120,
                ),
              ),
            ),
          ),
          Expanded(
            flex: 2,
            child: Padding(
              padding: const EdgeInsets.symmetric(
                horizontal: 30,
                vertical: 20,
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    "Khám phá những chiếc bánh ngon nhất từ hơn 1.000 tiệm và giao hàng nhanh tận nơi!",
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 40),
                  CustomButton(
                    text: "Đăng nhập",
                    color: const Color(0xFFF77E6E),
                    textColor: Colors.white,
                    onPressed: () {
                      Navigator.pushNamed(context, '/login');
                    },
                  ),
                  const SizedBox(height: 15),
                  CustomButton(
                    text: "Tạo tài khoản mới",
                    color: Colors.white,
                    textColor: const Color(0xFFF77E6E),
                    borderColor: const Color(0xFFF77E6E),
                    onPressed: () {
                      Navigator.pushNamed(context, '/signup');
                    },
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
