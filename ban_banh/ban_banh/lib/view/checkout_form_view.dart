import 'package:ban_banh/view/payment_view.dart';
import 'package:flutter/material.dart';

class CheckoutFormView extends StatefulWidget {
  final String userId;
  final String fullName;
  final List<dynamic> cartItems;
  final double totalPrice;

  const CheckoutFormView({
    super.key,
    required this.userId,
    required this.fullName,
    required this.cartItems,
    required this.totalPrice,
  });

  @override
  State<CheckoutFormView> createState() => _CheckoutFormViewState();
}

class _CheckoutFormViewState extends State<CheckoutFormView> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _addressController = TextEditingController();
  final _noteController = TextEditingController();
  late final List<TextEditingController> _itemNotesControllers;

  String? _selectedDistrict;
  String? _selectedWard;
  DateTime? _deliveryDateTime;
  String _paymentMethod = "COD";
  bool _isSubmitting = false;

  bool _isBirthdayCake = false; // Để kiểm tra xem có sản phẩm bánh sinh nhật không

  final Map<String, List<String>> _hcmData = const {
    "Quận 1": ["Phường Tân Định", "Phường Đa Kao", "Phường Bến Nghé", "Phường Bến Thành", "Phường Nguyễn Thái Bình", "Phường Phạm Ngũ Lão", "Phường Cầu Ông Lãnh", "Phường Cô Giang", "Phường Nguyễn Cư Trinh", "Phường Cầu Kho"],
    "Quận 3": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường Võ Thị Sáu", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14"],
    "Quận 4": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 6", "Phường 8", "Phường 9", "Phường 10", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 18"],
    "Quận 5": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14"],
    "Quận 6": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14"],
    "Quận 7": ["Phường Tân Thuận Đông", "Phường Tân Thuận Tây", "Phường Tân Kiểng", "Phường Tân Hưng", "Phường Bình Thuận", "Phường Tân Quy", "Phường Phú Thuận", "Phường Tân Phú", "Phường Tân Phong", "Phường Phú Mỹ"],
    "Quận 8": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16"],
    "Quận 10": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15"],
    "Quận 11": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16"],
    "Quận 12": ["Phường Thạnh Xuân", "Phường Thạnh Lộc", "Phường Hiệp Thành", "Phường Thới An", "Phường Tân Chánh Hiệp", "Phường An Phú Đông", "Phường Tân Thới Hiệp", "Phường Trung Mỹ Tây", "Phường Tân Hưng Thuận", "Phường Đông Hưng Thuận", "Phường Tân Thới Nhất"],
    "Quận Bình Tân": ["Phường Bình Hưng Hòa", "Phường Bình Hưng Hoà A", "Phường Bình Hưng Hoà B", "Phường Bình Trị Đông", "Phường Bình Trị Đông A", "Phường Bình Trị Đông B", "Phường Tân Tạo", "Phường Tân Tạo A", "Phường An Lạc", "Phường An Lạc A"],
    "Quận Bình Thạnh": ["Phường 1", "Phường 2", "Phường 3", "Phường 5", "Phường 6", "Phường 7", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 17", "Phường 19", "Phường 21", "Phường 22", "Phường 24", "Phường 25", "Phường 26", "Phường 27", "Phường 28"],
    "Quận Gò Vấp": ["Phường 1", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 17"],
    "Quận Phú Nhuận": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 17"],
    "Quận Tân Bình": ["Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15"],
    "Quận Tân Phú": ["Phường Tân Sơn Nhì", "Phường Tây Thạnh", "Phường Sơn Kỳ", "Phường Tân Quý", "Phường Tân Thành", "Phường Phú Thọ Hoà", "Phường Phú Thạnh", "Phường Phú Trung", "Phường Hoà Thạnh", "Phường Hiệp Tân", "Phường Tân Thới Hoà"],
    "Thành phố Thủ Đức": ["Phường Linh Xuân", "Phường Bình Chiểu", "Phường Linh Trung", "Phường Tam Bình", "Phường Tam Phú", "Phường Hiệp Bình Phước", "Phường Hiệp Bình Chánh", "Phường Linh Chiểu", "Phường Linh Tây", "Phường Linh Đông", "Phường Bình Thọ", "Phường Trường Thọ", "Phường Long Bình", "Phường Long Thạnh Mỹ", "Phường Tân Phú", "Phường Hiệp Phú", "Phường Tăng Nhơn Phú A", "Phường Tăng Nhơn Phú B", "Phường Phước Long B", "Phường Phước Long A", "Phường Trường Thạnh", "Phường Long Phước", "Phường Long Trường", "Phường Phú Hữu", "Phường Thảo Điền", "Phường An Phú", "Phường An Khánh", "Phường Bình Trưng Đông", "Phường Bình Trưng Tây", "Phường Cát Lái", "Phường Thạnh Mỹ Lợi", "Phường An Lợi Đông", "Phường Thủ Thiêm"],
    "Huyện Bình Chánh": ["Thị trấn Tân Túc", "Xã Phạm Văn Hai", "Xã Vĩnh Lộc A", "Xã Vĩnh Lộc B", "Xã Bình Lợi", "Xã Lê Minh Xuân", "Xã Tân Nhựt", "Xã Tân Kiên", "Xã Bình Hưng", "Xã Phong Phú", "Xã An Phú Tây", "Xã Hưng Long", "Xã Đa Phước", "Xã Tân Quý Tây", "Xã Bình Chánh", "Xã Quy Đức"],
    "Huyện Cần Giờ": ["Thị trấn Cần Thạnh", "Xã Bình Khánh", "Xã Tam Thôn Hiệp", "Xã An Thới Đông", "Xã Thạnh An", "Xã Long Hòa", "Xã Lý Nhơn"],
    "Huyện Củ Chi": ["Thị trấn Củ Chi", "Xã Phú Mỹ Hưng", "Xã An Phú", "Xã Trung Lập Thượng", "Xã An Nhơn Tây", "Xã Nhuận Đức", "Xã Phạm Văn Cội", "Xã Phú Hòa Đông", "Xã Trung Lập Hạ", "Xã Trung An", "Xã Phước Thạnh", "Xã Phước Hiệp", "Xã Tân An Hội", "Xã Phước Vĩnh An", "Xã Thái Mỹ", "Xã Tân Thạnh Tây", "Xã Hòa Phú", "Xã Tân Thạnh Đông", "Xã Bình Mỹ", "Xã Tân Phú Trung", "Xã Tân Thông Hội"],
    "Huyện Hóc Môn": ["Thị trấn Hóc Môn", "Xã Tân Hiệp", "Xã Nhị Bình", "Xã Đông Thạnh", "Xã Tân Thới Nhì", "Xã Thới Tam Thôn", "Xã Xuân Thới Sơn", "Xã Tân Xuân", "Xã Xuân Thới Đông", "Xã Trung Chánh", "Xã Xuân Thới Thượng", "Xã Bà Điểm"],
    "Huyện Nhà Bè": ["Thị trấn Nhà Bè", "Xã Phước Kiển", "Xã Phước Lộc", "Xã Nhơn Đức", "Xã Phú Xuân", "Xã Long Thới", "Xã Hiệp Phước"]

  };

  @override
  void initState() {
    super.initState();
    _nameController.text = widget.fullName;
    _itemNotesControllers =
        List.generate(widget.cartItems.length, (_) => TextEditingController());
    // Debug: In ra dữ liệu categoryName từ cartItems
    widget.cartItems.forEach((item) {
      print("Category : ${item['category']}");
    });
    // Kiểm tra xem có sản phẩm nào thuộc danh mục "Bánh Sinh Nhật"
    _isBirthdayCake = widget.cartItems.any((item) => item["category"]?.toLowerCase() == "bánh sinh nhật");
  }

  @override
  void dispose() {
    _nameController.dispose();
    _phoneController.dispose();
    _addressController.dispose();
    _noteController.dispose();
    for (var c in _itemNotesControllers) {
      c.dispose();
    }
    super.dispose();
  }

  // Kiểm tra và chỉ cho phép nhập số điện thoại 10 số, không nhập chữ
  String? _validatePhoneNumber(String? value) {
    if (value == null || value.isEmpty) {
      return "Vui lòng nhập số điện thoại";
    } else if (value.length != 10) {
      return "Số điện thoại phải có 10 chữ số";
    } else if (!RegExp(r'^[0-9]+$').hasMatch(value)) {
      return "Số điện thoại không được có chữ";
    }
    return null;
  }

  /// 🗓️ Chọn ngày và giờ giao hàng
  Future<void> _pickDateTime() async {
    final now = DateTime.now();
    final pickedDate = await showDatePicker(
      context: context,
      initialDate: now,
      firstDate: now,
      lastDate: now.add(const Duration(days: 14)),
      helpText: "Chọn ngày giao hàng",
      locale: const Locale('vi', 'VN'),
    );

    if (pickedDate == null) return;

    final pickedTime = await showTimePicker(
      context: context,
      initialTime: TimeOfDay.fromDateTime(now.add(const Duration(hours: 3))),
      helpText: "Chọn giờ giao hàng",
    );

    if (pickedTime != null) {
      final selectedDateTime = DateTime(
        pickedDate.year,
        pickedDate.month,
        pickedDate.day,
        pickedTime.hour,
        pickedTime.minute,
      );

      // ❗ Logic: đơn hôm nay chỉ được nhận sau 3 tiếng
      if (pickedDate.day == now.day &&
          selectedDateTime.isBefore(now.add(const Duration(hours: 3)))) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content: Text("Thời gian nhận hàng phải sau ít nhất 3 tiếng!"),
          backgroundColor: Colors.redAccent,
        ));
        return;
      }

      setState(() => _deliveryDateTime = selectedDateTime);
    }
  }

  Future<void> _submitOrder() async {
    if (!_formKey.currentState!.validate()) return;
    if (_deliveryDateTime == null) {
      ScaffoldMessenger.of(context)
          .showSnackBar(const SnackBar(content: Text("Vui lòng chọn thời gian giao hàng.")));
      return;
    }

    setState(() => _isSubmitting = true);

    // ✅ Dữ liệu sản phẩm
    final List<Map<String, dynamic>> orderDetails = [];
    for (int i = 0; i < widget.cartItems.length; i++) {
      final item = widget.cartItems[i];
      orderDetails.add({
        "productId": item["productId"],
        "name": item["productName"] ?? item["name"] ?? "Sản phẩm",
        "quantity": item["quantity"],
        "price": item["price"],
        "notes": _itemNotesControllers[i].text.trim(),
      });
    }

    // ✅ Tổng cộng có thêm phí vận chuyển
    final totalWithShipping = widget.totalPrice + 30000;

    final checkoutData = {
      "recipientName": _nameController.text.trim(),
      "recipientPhone": _phoneController.text.trim(),
      "specificAddress": _addressController.text.trim(),
      "district": _selectedDistrict ?? "",
      "ward": _selectedWard ?? "",
      "deliveryDateTime": _deliveryDateTime!.toIso8601String(),
      "notes": _noteController.text.trim(),
      "paymentMethod": _paymentMethod,
      "orderDetails": orderDetails,
      "totalPrice": totalWithShipping,
    };

    // ✅ Điều hướng đến PaymentView
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => PaymentView(orderData: checkoutData),
      ),
    );

    setState(() => _isSubmitting = false);
  }

  @override
  Widget build(BuildContext context) {
    final List<String> wards = _selectedDistrict != null
        ? List<String>.from(_hcmData[_selectedDistrict] ?? const <String>[])
        : const <String>[];

    return Scaffold(
      appBar: AppBar(
        title: const Text("Xác nhận đơn hàng"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text("Thông tin người nhận",
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
              _textField(_nameController, "Họ tên người nhận"),
              _textField(
                _phoneController,
                "Số điện thoại",
                type: TextInputType.phone,
                validator: _validatePhoneNumber,
              ),
              _textField(_addressController, "Địa chỉ cụ thể"),

              DropdownButtonFormField<String>(
                value: _selectedDistrict,
                decoration: const InputDecoration(labelText: "Quận/Huyện"),
                items: _hcmData.keys
                    .map<DropdownMenuItem<String>>(
                        (String d) => DropdownMenuItem<String>(value: d, child: Text(d)))
                    .toList(),
                onChanged: (v) {
                  setState(() {
                    _selectedDistrict = v;
                    _selectedWard = null;
                  });
                },
                validator: (v) => v == null ? "Vui lòng chọn quận/huyện" : null,
              ),
              DropdownButtonFormField<String>(
                value: _selectedWard,
                decoration: const InputDecoration(labelText: "Phường/Xã"),
                items: wards
                    .map<DropdownMenuItem<String>>(
                        (String w) => DropdownMenuItem<String>(value: w, child: Text(w)))
                    .toList(),
                onChanged: (v) => setState(() => _selectedWard = v),
                validator: (v) => v == null ? "Vui lòng chọn phường/xã" : null,
              ),

              const SizedBox(height: 20),
              const Text("Sản phẩm trong giỏ hàng",
                  style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
              for (int i = 0; i < widget.cartItems.length; i++)
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(10),
                    child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            "${widget.cartItems[i]["productName"] ?? widget.cartItems[i]["name"] ?? "Sản phẩm"} × ${widget.cartItems[i]["quantity"]}",
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                          Text("Giá: ${widget.cartItems[i]["price"]}₫"),
                          Text("Category: ${widget.cartItems[i]["category"]}"),

                          // Chỉ hiển thị ô ghi chú nếu sản phẩm thuộc danh mục "Bánh Sinh Nhật"
                          if (widget.cartItems[i]["category"] == "Bánh Sinh Nhật")
                            TextFormField(
                              controller: _itemNotesControllers[i],
                              decoration: const InputDecoration(
                                labelText: "Ghi chú chữ cần viết lên bánh và tuổi của người sinh nhật",
                              ),
                            ),
                        ]),
                  ),
                ),

              const SizedBox(height: 15),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      _deliveryDateTime == null
                          ? "Chưa chọn ngày & giờ nhận"
                          : "Nhận: ${_deliveryDateTime!.day}/${_deliveryDateTime!.month}/${_deliveryDateTime!.year} "
                          "${_deliveryDateTime!.hour}:${_deliveryDateTime!.minute.toString().padLeft(2, '0')}",
                    ),
                  ),
                  ElevatedButton(
                    onPressed: _pickDateTime,
                    style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFFF77E6E)),
                    child: const Text("Chọn thời gian"),
                  ),
                ],
              ),

              const SizedBox(height: 20),
              Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: TextFormField(
                  controller: _noteController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                      labelText: "Ghi chú đơn hàng (không bắt buộc)"),
                ),
              ),

              const SizedBox(height: 25),
              Center(
                child: ElevatedButton.icon(
                  onPressed: _isSubmitting ? null : _submitOrder,
                  icon: const Icon(Icons.check_circle),
                  label: const Text("XÁC NHẬN ĐẶT HÀNG",
                      style: TextStyle(fontWeight: FontWeight.bold)),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFFF77E6E),
                    padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 16),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _textField(TextEditingController c, String label,
      {TextInputType? type, int maxLines = 1, String? Function(String?)? validator}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: TextFormField(
        controller: c,
        maxLines: maxLines,
        keyboardType: type,
        decoration: InputDecoration(labelText: label),
        validator: validator ??
                (v) => v == null || v.isEmpty ? "Vui lòng nhập $label" : null,
      ),
    );
  }
}
