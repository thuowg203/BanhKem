import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import 'package:http/http.dart' as http;
import '../widgets/custom_button.dart';
import 'login_view.dart'; // Đảm bảo import LoginView
import 'welcome_view.dart';// Đảm bảo import WelcomeView

class SignupView extends StatelessWidget {
  const SignupView({super.key});

  Future<void> _register(
      BuildContext context, {
        required String fullName,
        required String email,
        required String password,
        required String phoneNumber,
        required String address,
      }) async {
    final url = Uri.parse("http://10.0.2.2:5006/api/accountapi/register");

    try {
      final response = await http.post(
        url,
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({
          "FullName": fullName,
          "Email": email,
          "Password": password,
          "PhoneNumber": phoneNumber,
          "Address": address,
        }),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Đăng ký thành công!")),
        );
        Navigator.pushNamed(context, '/login');
      } else {
        final data = jsonDecode(response.body);
        String errorMessage = "Đăng ký thất bại!";
        if (data is Map && data['message'] != null) {
          errorMessage = data['message'];
        }
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(errorMessage)),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi kết nối: $e")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final TextEditingController nameController = TextEditingController();
    final TextEditingController emailController = TextEditingController();
    final TextEditingController passwordController = TextEditingController();
    final TextEditingController confirmPasswordController = TextEditingController();
    final TextEditingController phoneController = TextEditingController();
    final TextEditingController addressController = TextEditingController();

    return Scaffold(
      appBar: AppBar(
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () {
            // Quay lại trang Welcome
            Navigator.pushReplacement(
              context,
              MaterialPageRoute(
                builder: (context) => const WelcomeView(),
              ),
            );
          },
        ),
        title: const Text("Đăng ký"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 30, vertical: 60),
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                "Đăng ký",
                style: TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFFF77E6E),
                ),
              ),
              const SizedBox(height: 10),
              Text(
                "Thêm thông tin của bạn để tạo tài khoản mới",
                style: TextStyle(color: Colors.grey[600]),
              ),
              const SizedBox(height: 30),

              // Họ và tên
              TextField(
                controller: nameController,
                decoration: InputDecoration(
                  labelText: "Họ và tên",
                  prefixIcon: const Icon(Icons.person_outline, color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Email
              TextField(
                controller: emailController,
                keyboardType: TextInputType.emailAddress,
                decoration: InputDecoration(
                  labelText: "Email của bạn",
                  prefixIcon: const Icon(Icons.email_outlined, color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Số điện thoại
              TextField(
                controller: phoneController,
                keyboardType: TextInputType.phone,
                decoration: InputDecoration(
                  labelText: "Số điện thoại",
                  prefixIcon: const Icon(Icons.phone_outlined, color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Địa chỉ
              TextField(
                controller: addressController,
                decoration: InputDecoration(
                  labelText: "Địa chỉ",
                  prefixIcon: const Icon(Icons.location_on_outlined, color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Mật khẩu
              TextField(
                controller: passwordController,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: "Mật khẩu",
                  prefixIcon: const Icon(Icons.lock_outline, color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Xác nhận mật khẩu
              TextField(
                controller: confirmPasswordController,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: "Xác nhận mật khẩu",
                  prefixIcon: const Icon(Icons.lock_outline, color: Color(0xFFF77E6E)),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
              const SizedBox(height: 30),

              // Nút đăng ký
              CustomButton(
                text: "Đăng ký",
                color: const Color(0xFFF77E6E),
                textColor: Colors.white,
                onPressed: () {
                  if (passwordController.text != confirmPasswordController.text) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text("Mật khẩu xác nhận không khớp")),
                    );
                    return;
                  }

                  _register(
                    context,
                    fullName: nameController.text,
                    email: emailController.text,
                    password: passwordController.text,
                    phoneNumber: phoneController.text,
                    address: addressController.text,
                  );
                },
              ),
              const SizedBox(height: 20),

              // Quay lại đăng nhập
              Center(
                child: RichText(
                  text: TextSpan(
                    text: "Đã có tài khoản? ",
                    style: TextStyle(color: Colors.grey[700]),
                    children: [
                      TextSpan(
                        text: "Đăng nhập",
                        style: TextStyle(
                          color: Color(0xFFF77E6E),
                          fontWeight: FontWeight.bold,
                        ),
                        // Điều hướng đến LoginView
                        recognizer: TapGestureRecognizer()
                          ..onTap = () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => const LoginView(),
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
