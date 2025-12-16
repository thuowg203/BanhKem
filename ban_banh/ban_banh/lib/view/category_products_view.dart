import 'package:flutter/material.dart';
import '../services/api_service.dart';
import 'product_detail_view.dart';

class CategoryProductsView extends StatefulWidget {
  final Map category;
  final String userId;

  const CategoryProductsView({
    super.key,
    required this.category,
    required this.userId,
  });

  @override
  State<CategoryProductsView> createState() => _CategoryProductsViewState();
}

class _CategoryProductsViewState extends State<CategoryProductsView> {
  List products = [];
  bool isLoading = true;

  @override
  void initState() {
    super.initState();
    fetchProductsByCategory();
  }

  // 🧁 Tạo link ảnh đầy đủ
  String buildImageUrl(dynamic raw) {
    if (raw == null) return '';
    final s = raw.toString();
    if (s.startsWith('http')) return s;
    const base = 'http://10.0.2.2:5006';
    final path = s.startsWith('/') ? s : '/$s';
    return '$base$path';
  }

  //Gọi API lấy sản phẩm theo danh mục
  Future<void> fetchProductsByCategory() async {
    try {
      final categoryId = widget.category['id'] ?? widget.category['maLoai'];
      if (categoryId == null) {
        setState(() {
          isLoading = false;
          products = [];
        });
        return;
      }

      final data = await ApiService.getProductsByCategory(categoryId);
      setState(() {
        products = data;
        isLoading = false;
      });
    } catch (e) {
      debugPrint("Lỗi khi tải sản phẩm theo danh mục: $e");
      setState(() => isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final catName = widget.category['tenLoai'] ?? 'Danh mục';

    return Scaffold(
      appBar: AppBar(
        title: Text(catName),
        backgroundColor: const Color(0xFFF77E6E),
        foregroundColor: Colors.white,
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : products.isEmpty
          ? Center(
        child: Text(
          "Không có sản phẩm trong danh mục \"$catName\".",
          style: const TextStyle(fontSize: 16, color: Colors.grey),
          textAlign: TextAlign.center,
        ),
      )
          : Padding(
        padding: const EdgeInsets.all(16.0),
        child: GridView.builder(
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2,
            mainAxisExtent: 220,
            crossAxisSpacing: 15,
            mainAxisSpacing: 15,
          ),
          itemCount: products.length,
          itemBuilder: (context, index) {
            final p = products[index] as Map<String, dynamic>;
            final imageUrl = buildImageUrl(p['imageUrl']);
            final name = (p['name'] ?? 'Không rõ tên').toString();
            final price = (p['price'] ?? 0).toString();

            return GestureDetector(
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => ProductDetailView(
                      product: p,
                      userId: widget.userId,
                    ),
                  ),
                );
              },
              child: Container(
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(15),
                  color: Colors.white,
                  boxShadow: [
                    BoxShadow(
                      color: Colors.grey.withOpacity(0.15),
                      blurRadius: 6,
                      spreadRadius: 2,
                    ),
                  ],
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    ClipRRect(
                      borderRadius: const BorderRadius.vertical(
                        top: Radius.circular(15),
                      ),
                      child: Image.network(
                        imageUrl,
                        height: 120,
                        width: double.infinity,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) => Image.asset(
                          "assets/images/cake_default.png",
                          height: 120,
                          width: double.infinity,
                          fit: BoxFit.cover,
                        ),
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.all(8.0),
                      child: Text(
                        name,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 15,
                        ),
                      ),
                    ),
                    Padding(
                      padding:
                      const EdgeInsets.symmetric(horizontal: 8.0),
                      child: Text(
                        "$price VNĐ",
                        style: const TextStyle(
                          color: Color(0xFFF77E6E),
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
