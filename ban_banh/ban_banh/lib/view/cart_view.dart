import 'package:flutter/material.dart';
import '../services/api_service.dart';
import 'checkout_form_view.dart';

class CartView extends StatefulWidget {
  final String userId;
  final String fullName;

  const CartView({
    super.key,
    required this.userId,
    required this.fullName,
  });

  @override
  State<CartView> createState() => _CartViewState();
}

class _CartViewState extends State<CartView> {
  List<dynamic> _cartItems = [];
  bool _loading = true;
  double _totalPrice = 0;

  @override
  void initState() {
    super.initState();
    _loadCart();
  }

  String buildImageUrl(String? raw) {
    if (raw == null || raw.isEmpty) return '';
    if (raw.startsWith('http')) return raw;
    return 'http://10.0.2.2:5006${raw.startsWith('/') ? raw : '/$raw'}';
  }

  String formatPrice(double price) {
    return price
        .toStringAsFixed(0)
        .replaceAllMapped(RegExp(r'(\d)(?=(\d{3})+(?!\d))'), (m) => '${m[1]}.');
  }

  Future<void> _loadCart() async {
    setState(() => _loading = true);
    try {
      final data = await ApiService.getCart(widget.userId);
      setState(() {
        _cartItems = data["items"] ?? [];
        _totalPrice = (data["totalValue"] as num?)?.toDouble() ?? 0;
      });
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi tải giỏ hàng: $e")),
      );
    } finally {
      setState(() => _loading = false);
    }
  }

  Future<void> _updateQuantity(int productId, int newQty) async {
    if (newQty < 1) return;
    try {
      await ApiService.updateQuantity(
        userId: widget.userId,
        productId: productId,
        quantity: newQty,
      );
      await _loadCart();
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi cập nhật số lượng: $e")),
      );
    }
  }

  Future<void> _removeItem(int productId) async {
    try {
      await ApiService.removeFromCart(
        userId: widget.userId,
        productId: productId,
      );
      await _loadCart();
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi xóa sản phẩm: $e")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("Giỏ hàng của ${widget.fullName}"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _cartItems.isEmpty
          ? const Center(child: Text("🛒 Giỏ hàng trống"))
          : RefreshIndicator(
        onRefresh: _loadCart,
        child: ListView(
          padding: const EdgeInsets.all(12),
          children: [
            ..._cartItems.map((item) {
              final productId = item["productId"];
              final name = item["productName"] ?? "Không rõ";
              final price = (item["price"] as num?)?.toDouble() ?? 0;
              final qty = (item["quantity"] as num?)?.toInt() ?? 1;
              final imageUrl = buildImageUrl(item["imageUrl"]);
              final category = item["category"];

              return Card(
                elevation: 3,
                margin: const EdgeInsets.only(bottom: 12),
                child: Padding(
                  padding: const EdgeInsets.all(8.0),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: Image.network(
                          imageUrl,
                          width: 60,
                          height: 60,
                          fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => const Icon(
                            Icons.broken_image,
                            size: 42,
                            color: Colors.grey,
                          ),
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(name,
                                style: const TextStyle(
                                    fontWeight: FontWeight.bold,
                                    fontSize: 16)),
                            const SizedBox(height: 4),
                            Text(
                              "${formatPrice(price)}₫",
                              style: const TextStyle(
                                  color: Color(0xFFF77E6E),
                                  fontWeight: FontWeight.w500),
                            ),
                            Text(
                              "Category: ${item['category'] ?? 'Chưa có thông tin'}",
                              style: const TextStyle(fontSize: 14, color: Colors.grey),
                            ),
                          ],
                        ),
                      ),
                      Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              IconButton(
                                icon: const Icon(Icons.remove),
                                onPressed: () =>
                                    _updateQuantity(productId, qty - 1),
                              ),
                              Text(
                                "$qty",
                                style: const TextStyle(
                                    fontSize: 16, fontWeight: FontWeight.bold),
                              ),
                              IconButton(
                                icon: const Icon(Icons.add),
                                onPressed: () =>
                                    _updateQuantity(productId, qty + 1),
                              ),
                            ],
                          ),
                          IconButton(
                            icon: const Icon(Icons.delete, color: Colors.redAccent),
                            onPressed: () => _removeItem(productId),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              );
            }),
            const Divider(),
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 10),
              child: Text(
                "Tổng cộng: ${formatPrice(_totalPrice)}₫",
                textAlign: TextAlign.right,
                style: const TextStyle(
                    fontSize: 18, fontWeight: FontWeight.bold),
              ),
            ),
            ElevatedButton(
              onPressed: () async {
                final result = await Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => CheckoutFormView(
                      userId: widget.userId,
                      fullName: widget.fullName,
                      cartItems: _cartItems,
                      totalPrice: _totalPrice,
                    ),
                  ),
                );
                if (result == true) {
                  _loadCart();
                }
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFFF77E6E),
                padding: const EdgeInsets.symmetric(horizontal: 30, vertical: 14),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              ),
              child: const Text("THANH TOÁN",
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white,)),
            ),
          ],
        ),
      ),
    );
  }
}
