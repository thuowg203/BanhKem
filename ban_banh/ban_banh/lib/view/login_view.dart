import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import '../services/api_service.dart';
import '../widgets/custom_button.dart';
import 'home_view.dart';
import 'signup_view.dart'; // Đảm bảo đã import SignupView

class LoginView extends StatefulWidget {
  const LoginView({super.key});

  @override
  State<LoginView> createState() => _LoginViewState();
}

class _LoginViewState extends State<LoginView> {
  final TextEditingController emailController = TextEditingController();
  final TextEditingController passwordController = TextEditingController();
  bool isLoading = false;

  void _login() async {
    final email = emailController.text.trim();
    final password = passwordController.text.trim();

    if (email.isEmpty || password.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Vui lòng nhập đầy đủ thông tin.")),
      );
      return;
    }

    setState(() => isLoading = true);

    try {
      final result = await ApiService.login(email: email, password: password);

      // Nếu đăng nhập thành công
      if (result.isNotEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Đăng nhập thành công!")),
        );

        // Chuyển sang trang Home
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(
            builder: (context) => HomeView(
              fullName: result["fullName"],
              userId: result["userId"],
            ),
          ),
        );
      }
    } catch (e) {
      // Nếu sai thông tin hoặc lỗi server
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi đăng nhập: $e")),
      );
    } finally {
      setState(() => isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 30, vertical: 60),
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Center(
                child: Text(
                  "Đăng nhập",
                  style: TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: Color(0xFFF77E6E),
                  ),
                ),
              ),
              const SizedBox(height: 10),
              Text(
                "Nhập thông tin tài khoản của bạn để tiếp tục",
                style: TextStyle(color: Colors.grey[600]),
              ),
              const SizedBox(height: 30),

              // Ô nhập email
              TextField(
                controller: emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: InputDecoration(
                  labelText: "Email của bạn",
                  prefixIcon: const Icon(Icons.email_outlined,
                      color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Ô nhập mật khẩu
              TextField(
                controller: passwordController,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: "Mật khẩu",
                  prefixIcon: const Icon(Icons.lock_outline,
                      color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Nút đăng nhập
              CustomButton(
                text: isLoading ? "Đang đăng nhập..." : "Đăng nhập",
                color: const Color(0xFFF77E6E),
                textColor: Colors.white,
                onPressed: isLoading ? () {} : _login,
              ),
              const SizedBox(height: 20),

              // Quên mật khẩu
              Center(
                child: TextButton(
                  onPressed: () {},
                  child: const Text(
                    "Quên mật khẩu?",
                    style: TextStyle(color: Colors.grey),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Chưa có tài khoản?
              Center(
                child: RichText(
                  text: TextSpan(
                    text: "Chưa có tài khoản? ",
                    style: TextStyle(color: Colors.grey[700]),
                    children: [
                      TextSpan(
                        text: "Đăng ký ngay",
                        style: const TextStyle(
                          color: Color(0xFFF76C2E),
                          fontWeight: FontWeight.bold,
                        ),
                        // Điều hướng đến SignupView
                        recognizer: TapGestureRecognizer()
                          ..onTap = () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => const SignupView(),
                              ),
                            );
                          },
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
