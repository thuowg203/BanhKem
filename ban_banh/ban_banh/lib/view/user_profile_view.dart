import 'package:flutter/material.dart';
import '../services/api_service.dart';

class UserProfileView extends StatefulWidget {
  final String userId;
  final String fullName;

  const UserProfileView({
    super.key,
    required this.userId,
    required this.fullName,
  });

  @override
  State<UserProfileView> createState() => _UserProfileViewState();
}

class _UserProfileViewState extends State<UserProfileView> {
  final _formKey = GlobalKey<FormState>();
  final TextEditingController _fullNameController = TextEditingController();
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _phoneController = TextEditingController();
  final TextEditingController _addressController = TextEditingController();

  bool _loading = true;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _fetchProfile();
  }

  Future<void> _fetchProfile() async {
    try {
      final data = await ApiService.getUserProfile(widget.userId);
      setState(() {
        _fullNameController.text = data["fullName"] ?? "";
        _emailController.text = data["email"] ?? "";
        _phoneController.text = data["phoneNumber"] ?? "";
        _addressController.text = data["address"] ?? "";
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi tải thông tin: $e")),
      );
    }
  }

  Future<void> _saveProfile() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _saving = true);
    try {
      final ok = await ApiService.updateUserProfile(
        userId: widget.userId,
        fullName: _fullNameController.text.trim(),
        email: _emailController.text.trim(),
        phoneNumber: _phoneController.text.trim(),
        address: _addressController.text.trim(),
      );

      if (ok) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text("Cập nhật hồ sơ thành công"),
            backgroundColor: Colors.green,
          ),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text("Lỗi cập nhật hồ sơ: $e"),
          backgroundColor: Colors.redAccent,
        ),
      );
    } finally {
      setState(() => _saving = false);
    }
  }

  void _logout() {
    ApiService.setToken(""); // Xoá token
    Navigator.of(context).pushNamedAndRemoveUntil("/login", (route) => false);
  }

  @override
  Widget build(BuildContext context) {
    const kAccent = Color(0xFFF77E6E);

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          "Thông tin tài khoản",
          style: TextStyle(fontWeight: FontWeight.w600),
        ),
        backgroundColor: kAccent,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: _logout,
          )
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: kAccent))
          : SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Center(
                child: CircleAvatar(
                  radius: 45,
                  backgroundColor: Color(0xFFFFE7E0),
                  child: Icon(Icons.person, size: 50, color: kAccent),
                ),
              ),
              const SizedBox(height: 20),
              _buildField(
                label: "Họ và tên",
                controller: _fullNameController,
                validator: (v) =>
                v == null || v.isEmpty ? "Vui lòng nhập họ tên" : null,
              ),
              _buildField(
                label: "Email",
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                validator: (v) => v == null || v.isEmpty
                    ? "Vui lòng nhập email"
                    : (!v.contains("@")
                    ? "Email không hợp lệ"
                    : null),
              ),
              _buildField(
                label: "Số điện thoại",
                controller: _phoneController,
                keyboardType: TextInputType.phone,
                validator: (v) => v == null || v.isEmpty
                    ? "Vui lòng nhập số điện thoại"
                    : null,
              ),
              _buildField(
                label: "Địa chỉ",
                controller: _addressController,
                validator: (v) => v == null || v.isEmpty
                    ? "Vui lòng nhập địa chỉ"
                    : null,
              ),
              const SizedBox(height: 25),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: kAccent,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10),
                    ),
                  ),
                  onPressed: _saving ? null : _saveProfile,
                  child: _saving
                      ? const CircularProgressIndicator(
                    color: Colors.white,
                  )
                      : const Text(
                    "LƯU THAY ĐỔI",
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Center(
                child: TextButton.icon(
                  onPressed: _logout,
                  icon: const Icon(Icons.logout, color: Colors.redAccent),
                  label: const Text(
                    "Đăng xuất",
                    style: TextStyle(
                      color: Colors.redAccent,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildField({
    required String label,
    required TextEditingController controller,
    String? Function(String?)? validator,
    TextInputType? keyboardType,
  }) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: TextFormField(
        controller: controller,
        validator: validator,
        keyboardType: keyboardType,
        decoration: InputDecoration(
          labelText: label,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          focusedBorder: const OutlineInputBorder(
            borderSide: BorderSide(color: Color(0xFFF77E6E), width: 2),
          ),
          contentPadding:
          const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        ),
      ),
    );
  }
}
