import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

class ProductDetailView extends StatefulWidget {
  final Map<String, dynamic> product;
  final String userId;

  const ProductDetailView({super.key, required this.product, required this.userId});

  @override
  State<ProductDetailView> createState() => _ProductDetailViewState();
}

class _ProductDetailViewState extends State<ProductDetailView> {
  static const String _base = 'http://10.0.2.2:5006';
  static const kAccent = Color(0xFFF77E6E);

  int _quantity = 1;
  String _activeSize = '12 CM';
  late String _mainImage;
  late List<String> _thumbnails;

  bool _descOpen = true;
  bool _specialOpen = false;

  String buildImageUrl(dynamic raw) {
    final s = (raw ?? '').toString().trim();
    if (s.isEmpty) return '';
    if (s.startsWith('http')) return s;
    final path = s.startsWith('/') ? s : '/$s';
    return '$_base$path';
  }

  @override
  void initState() {
    super.initState();
    final imageRaw =
        widget.product['imageUrl'] ?? widget.product['image'] ?? widget.product['hinhAnh'];
    _mainImage = buildImageUrl(imageRaw);
    _thumbnails = List.generate(4, (_) => _mainImage);
  }

  // Thêm vào giỏ hàng
  Future<void> addToCart(int productId, int quantity, String name) async {
    final url = Uri.parse('$_base/api/CartApi/add');
    final imageUrl = widget.product["imageUrl"] ?? widget.product["image"] ?? widget.product["hinhAnh"] ?? "";

    final body = jsonEncode({
      "productId": productId,
      "name": name,
      "quantity": quantity,
      "price": widget.product["price"] ?? widget.product["gia"] ?? 0,
      "imageUrl": imageUrl,
      "userId": widget.userId,
    });

    try {
      final res = await http.post(
        url,
        headers: {"Content-Type": "application/json"},
        body: body,
      );

      if (res.statusCode == 200) {
        _onAddToCartResponse(true);
      } else {
        _onAddToCartResponse(false);
      }

    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi kết nối API: $e")),
      );
    }
  }

  // Phương thức hiển thị thông báo overlay
  void _showOverlayMessage(String message, Color color) {
    final overlay = Overlay.of(context);
    final overlayEntry = OverlayEntry(
      builder: (context) => Positioned(
        top: 50, // Vị trí của thông báo
        left: MediaQuery.of(context).size.width * 0.2, // Canh trái
        child: Material(
          color: Colors.transparent,
          child: Container(
            padding: EdgeInsets.symmetric(horizontal: 20, vertical: 10),
            decoration: BoxDecoration(
              color: color, // Sử dụng màu của button "Thêm vào giỏ"
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              message,
              style: const TextStyle(color: Colors.white, fontSize: 16),
            ),
          ),
        ),
      ),
    );

    overlay.insert(overlayEntry);

    // Remove overlay after a delay
    Future.delayed(Duration(seconds: 2), () {
      overlayEntry.remove(); // Remove the overlay
    });
  }

  void _onAddToCartResponse(bool success) {
    // Thay đổi màu thông báo cho phù hợp với màu của button
    Color overlayColor = kAccent; // Màu của nút "Thêm vào giỏ"

    if (!success) {
      overlayColor = Colors.redAccent; // Nếu có lỗi thì màu đỏ
    }

    _showOverlayMessage(success
        ? "Đã thêm sản phẩm vào giỏ hàng"
        : "Lỗi thêm vào giỏ hàng", overlayColor);
  }


  @override
  Widget build(BuildContext context) {
    final name = (widget.product['name'] ?? widget.product['tenSP'] ?? 'Chi Tiết Sản Phẩm').toString();
    final price = (widget.product['price'] ?? widget.product['gia'] ?? 0);
    final desc = (widget.product['description'] ?? 'Mô tả đang cập nhật').toString();
    final badge = (widget.product['badge'] ?? 'Sản phẩm không có thông tin đặc biệt.').toString();
    final productId = widget.product['id'] ?? widget.product['maSP'] ?? 0;

    final priceText = _formatPrice(price);

    return Scaffold(
      backgroundColor: const Color(0xFFFCFAF8),
      appBar: AppBar(
        elevation: 0,
        backgroundColor: const Color(0xFFFCFAF8),
        foregroundColor: Colors.black,
        title: Text(
          name,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
      ),

      // 🧁 Thanh Thêm vào giỏ hàng
      bottomNavigationBar: SafeArea(
        top: false,
        child: Container(
          padding: const EdgeInsets.fromLTRB(16, 10, 16, 16),
          decoration: BoxDecoration(
            color: Colors.white,
            boxShadow: [BoxShadow(color: Colors.black.withOpacity(.04), blurRadius: 12, offset: const Offset(0, -2))],
            borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
          ),
          child: Row(
            children: [
              _QtyCompact(
                value: _quantity,
                onMinus: () => setState(() => _quantity = (_quantity - 1).clamp(1, 999)),
                onPlus: () => setState(() => _quantity = (_quantity + 1).clamp(1, 999)),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    backgroundColor: kAccent, // Màu nền của nút
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    elevation: 0,
                  ),
                  onPressed: () => addToCart(productId, _quantity, name),
                  icon: const Icon(
                    Icons.shopping_cart_outlined, // Icon giỏ hàng
                    color: Colors.white, // Màu icon trắng
                    size: 20, // Kích thước icon
                  ),
                  label: Text(
                    'THÊM VÀO GIỎ • $priceText', // Nội dung nút
                    style: const TextStyle(
                      letterSpacing: .5,
                      fontWeight: FontWeight.w700,
                      color: Colors.white, // Màu chữ trắng
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),

      // Nội dung chi tiết
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 24 + 72),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _HeroImage(url: _mainImage),
              const SizedBox(height: 18),
              Text(
                name,
                style: const TextStyle(
                    fontFamily: 'Georgia', fontSize: 24, color: kAccent, fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 6),
              Text(
                priceText,
                style: const TextStyle(fontSize: 20, color: kAccent, fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 12),
              const SizedBox(height: 10),

              _MobileAccordion(
                title: 'MÔ TẢ',
                isOpen: _descOpen,
                onToggle: () => setState(() => _descOpen = !_descOpen),
                child: Text(desc, style: const TextStyle(fontSize: 15.5, height: 1.55, color: Colors.black87)),
              ),
              _MobileAccordion(
                title: 'CÓ GÌ ĐẶC BIỆT?',
                isOpen: _specialOpen,
                onToggle: () => setState(() => _specialOpen = !_specialOpen),
                child: Text(badge, style: const TextStyle(fontSize: 15.5, height: 1.55, color: Colors.black87)),
              ),

              const SizedBox(height: 8),
              const Text('Bảo quản', style: TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
              const SizedBox(height: 6),
              const _Bullet(text: '❄️ Bảo quản lạnh trong bao bì/hộp bánh để tránh ám mùi.'),
              const _Bullet(text: '🗓️ Dùng trong vòng 3 ngày.'),
            ],
          ),
        ),
      ),
    );
  }

  String _formatPrice(dynamic value) {
    final num n = (value is num) ? value : num.tryParse(value.toString()) ?? 0;
    final s = n.toStringAsFixed(0).replaceAllMapped(
        RegExp(r'(\d)(?=(\d{3})+(?!\d))'), (m) => '${m[1]}.');
    return '$s₫';
  }
}

// ------------------ Các widget con giữ nguyên ------------------
class _HeroImage extends StatelessWidget {
  final String url;
  const _HeroImage({required this.url});
  @override
  Widget build(BuildContext context) {
    return AspectRatio(
      aspectRatio: 4 / 3,
      child: ClipRRect(
        borderRadius: BorderRadius.circular(16),
        child: Image.network(
          url,
          fit: BoxFit.cover,
          errorBuilder: (_, __, ___) =>
          const Center(child: Icon(Icons.broken_image, size: 42, color: Colors.grey)),
        ),
      ),
    );
  }
}

class _SizeChip extends StatelessWidget {
  final String text;
  final bool active;
  final VoidCallback onTap;
  const _SizeChip({required this.text, required this.active, required this.onTap});
  @override
  Widget build(BuildContext context) {
    const kAccent = Color(0xFFF77E6E);
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: InkWell(
        borderRadius: BorderRadius.circular(999),
        onTap: onTap,
        child: Ink(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
          decoration: BoxDecoration(
            color: active ? kAccent : Colors.white,
            border: Border.all(color: const Color(0xFFDDDDDD)),
            borderRadius: BorderRadius.circular(999),
          ),
          child: Text(
            text,
            style: TextStyle(
              color: active ? Colors.white : const Color(0xFF333333),
              fontWeight: FontWeight.w600,
              fontSize: 13,
            ),
          ),
        ),
      ),
    );
  }
}

class _MobileAccordion extends StatelessWidget {
  final String title;
  final bool isOpen;
  final VoidCallback onToggle;
  final Widget child;
  const _MobileAccordion({required this.title, required this.isOpen, required this.onToggle, required this.child});
  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        const Divider(height: 24, color: Color(0xFFEFEFEF)),
        InkWell(
          onTap: onToggle,
          child: Row(
            children: [
              Expanded(child: Text(title, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15.5))),
              AnimatedRotation(
                duration: const Duration(milliseconds: 180),
                turns: isOpen ? .5 : 0,
                child: const Icon(Icons.keyboard_arrow_down),
              ),
            ],
          ),
        ),
        const SizedBox(height: 8),
        AnimatedCrossFade(
          firstChild: const SizedBox.shrink(),
          secondChild: Padding(padding: const EdgeInsets.only(bottom: 8), child: child),
          crossFadeState: isOpen ? CrossFadeState.showSecond : CrossFadeState.showFirst,
          duration: const Duration(milliseconds: 200),
        ),
      ],
    );
  }
}

class _QtyCompact extends StatelessWidget {
  final int value;
  final VoidCallback onMinus;
  final VoidCallback onPlus;
  const _QtyCompact({required this.value, required this.onMinus, required this.onPlus});
  @override
  Widget build(BuildContext context) {
    return Container(
      height: 46,
      decoration: BoxDecoration(
        border: Border.all(color: const Color(0xFFDDDDDD)),
        borderRadius: BorderRadius.circular(12),
        color: Colors.white,
      ),
      child: Row(
        children: [
          _btn('-', onMinus),
          Container(
            width: 46,
            alignment: Alignment.center,
            decoration: const BoxDecoration(
              border: Border(
                left: BorderSide(color: Color(0xFFDDDDDD)),
                right: BorderSide(color: Color(0xFFDDDDDD)),
              ),
            ),
            child: Text('$value', style: const TextStyle(fontWeight: FontWeight.w700)),
          ),
          _btn('+', onPlus),
        ],
      ),
    );
  }

  Widget _btn(String text, VoidCallback onTap) {
    return SizedBox(
      width: 46,
      height: 46,
      child: InkWell(
        onTap: onTap,
        child: Center(
          child: Text(text, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
        ),
      ),
    );
  }
}

class _Bullet extends StatelessWidget {
  final String text;
  const _Bullet({required this.text});
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 6),
      child: Text(text, style: const TextStyle(color: Colors.black54, fontSize: 14.5, height: 1.45)),
    );
  }
}
