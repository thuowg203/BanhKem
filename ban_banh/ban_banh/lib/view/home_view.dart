import 'package:flutter/material.dart';
import 'package:ban_banh/view/product_detail_view.dart';
import 'package:ban_banh/view/cart_view.dart';
import 'package:ban_banh/view/ChatView.dart';
import 'package:ban_banh/view/user_profile_view.dart';
import 'package:geocoding/geocoding.dart';
import 'package:geolocator/geolocator.dart';
import '../services/api_service.dart';
import 'order_history_view.dart';

class BannerSlider extends StatefulWidget {
  const BannerSlider({super.key});

  @override
  State<BannerSlider> createState() => _BannerSliderState();
}

class _BannerSliderState extends State<BannerSlider> {
  final PageController _pageController = PageController();
  int _currentPage = 0;
  final List<String> _banners = [
    'assets/images/banner1.jpg',
    'assets/images/banner2.jpg',
    'assets/images/banner3.jpg',
  ];

  @override
  void initState() {
    super.initState();
    Future.delayed(const Duration(seconds: 3), autoSlide);
  }

  void autoSlide() {
    if (!mounted) return;
    setState(() {
      _currentPage = (_currentPage + 1) % _banners.length;
    });
    _pageController.animateToPage(
      _currentPage,
      duration: const Duration(milliseconds: 600),
      curve: Curves.easeInOut,
    );
    Future.delayed(const Duration(seconds: 3), autoSlide);
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      alignment: Alignment.bottomCenter,
      children: [
        PageView.builder(
          controller: _pageController,
          itemCount: _banners.length,
          itemBuilder: (context, index) {
            return Image.asset(
              _banners[index],
              fit: BoxFit.cover,
              width: double.infinity,
            );
          },
          onPageChanged: (index) {
            setState(() => _currentPage = index);
          },
        ),
        Positioned(
          bottom: 8,
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(_banners.length, (index) {
              return Container(
                margin: const EdgeInsets.symmetric(horizontal: 4),
                width: _currentPage == index ? 10 : 6,
                height: _currentPage == index ? 10 : 6,
                decoration: BoxDecoration(
                  color: _currentPage == index ? Colors.white : Colors.white54,
                  shape: BoxShape.circle,
                ),
              );
            }),
          ),
        ),
      ],
    );
  }
}

class HomeView extends StatefulWidget {
  final String fullName;
  final String userId;

  const HomeView({super.key, required this.fullName, required this.userId});

  @override
  State<HomeView> createState() => _HomeViewState();
}

class _HomeViewState extends State<HomeView> {
  List categories = [];
  List products = [];
  List filteredProducts = [];
  bool isLoading = true;
  String searchQuery = "";
  String userLocation = "Đang xác định vị trí...";

  @override
  void initState() {
    super.initState();
    fetchData();
    getUserLocation();
  }

  ///Lấy vị trí người dùng
  Future<void> getUserLocation() async {
    bool serviceEnabled;
    LocationPermission permission;

    serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      print("⚠️ GPS đang tắt");
      setState(() {
        userLocation = "Vui lòng bật GPS để xem vị trí.";
      });
      return;
    }

    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) {
        print("Quyền vị trí bị từ chối.");
        setState(() {
          userLocation = "Quyền truy cập vị trí bị từ chối.";
        });
        return;
      }
    }

    if (permission == LocationPermission.deniedForever) {
      print("Quyền vị trí bị từ chối vĩnh viễn.");
      setState(() {
        userLocation = "Cần bật quyền vị trí trong Cài đặt.";
      });
      return;
    }

    // Lấy toạ độ thực tế
    Position position = await Geolocator.getCurrentPosition(
      desiredAccuracy: LocationAccuracy.high,
    );
    print("Vị trí: ${position.latitude}, ${position.longitude}");

    List<Placemark> placemarks = await placemarkFromCoordinates(
      position.latitude,
      position.longitude,
    );

    if (placemarks.isNotEmpty) {
      final place = placemarks.first;
      setState(() {
        userLocation =
        "${place.subAdministrativeArea ?? ''}, ${place.administrativeArea ?? ''}";
      });
    } else {
      setState(() {
        userLocation = "Không xác định được địa điểm.";
      });
    }
  }


  String buildImageUrl(dynamic raw) {
    if (raw == null) return '';
    final s = raw.toString();
    if (s.isEmpty) return '';
    if (s.startsWith('http')) return s;
    const base = 'http://10.0.2.2:5006';
    final path = s.startsWith('/') ? s : '/$s';
    return '$base$path';
  }

  Future<void> fetchData() async {
    try {
      final catData = await ApiService.getCategories();
      final prodData = await ApiService.getProducts();
      setState(() {
        categories = catData;
        products = prodData;
        filteredProducts = prodData;
        isLoading = false;
      });
    } catch (e) {
      debugPrint("Lỗi khi tải dữ liệu: $e");
      setState(() => isLoading = false);
    }
  }

  void openCart() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => CartView(
          userId: widget.userId,
          fullName: widget.fullName,
        ),
      ),
    );
  }



  void openChat() {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ChatView(
          userId: widget.userId,
          fullName: widget.fullName,
        ),
      ),
    );
  }

  void filterProducts(String query) {
    final lowerQuery = query.toLowerCase();
    setState(() {
      searchQuery = query;
      if (query.isEmpty) {
        filteredProducts = products;
      } else {
        filteredProducts = products.where((p) {
          final name = (p['name'] ?? p['tenSP'] ?? '').toString().toLowerCase();
          return name.contains(lowerQuery);
        }).toList();
      }
    });
  }

  void showAllCategories() {
    showModalBottomSheet(
      context: context,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) {
        return Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                "Tất cả danh mục",
                style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 15),
              Expanded(
                child: ListView.builder(
                  itemCount: categories.length,
                  itemBuilder: (context, index) {
                    final cat = categories[index] as Map;
                    final catName =
                    (cat['tenLoai'] ?? cat['category']?['tenLoai'] ?? 'Không rõ').toString();
                    final catId = cat['id'] ?? cat['categoryId'];
                    return ListTile(
                      leading: const Icon(Icons.cake, color: Color(0xFFF77E6E)),
                      title: Text(catName),
                      onTap: () {
                        Navigator.pop(context);
                        if (catId == null) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(
                              content: Text("Không tìm thấy ID danh mục."),
                            ),
                          );
                          return;
                        }
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => CategoryProductsView(
                              categoryId: catId is int ? catId : int.tryParse('$catId') ?? -1,
                              categoryName: catName,
                              buildImageUrl: buildImageUrl,
                              userId: widget.userId,
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  void openCategory(Map cat) {
    final catName =
    (cat['tenLoai'] ?? cat['category']?['tenLoai'] ?? 'Danh mục').toString();
    final catId = cat['id'] ?? cat['categoryId'];
    if (catId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Không tìm thấy ID danh mục.")),
      );
      return;
    }
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => CategoryProductsView(
          categoryId: catId is int ? catId : int.tryParse('$catId') ?? -1,
          categoryName: catName,
          buildImageUrl: buildImageUrl,
          userId: widget.userId,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar:AppBar(
        automaticallyImplyLeading: false,
        elevation: 0,
        backgroundColor: Colors.white,
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              "Xin chào, ${widget.fullName}",
              style: const TextStyle(
                color: Colors.black,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 2),
            Row(
              children: [
                const Icon(Icons.location_on, size: 16, color: Colors.redAccent),
                const SizedBox(width: 4),
                Flexible(
                  child: Text(
                    userLocation,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 13, color: Colors.black54),
                  ),
                ),
              ],
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.person_outline, color: Colors.black),
            onPressed: () {
              showModalBottomSheet(
                context: context,
                shape: const RoundedRectangleBorder(
                  borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
                ),
                builder: (BuildContext context) {
                  return SafeArea(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const SizedBox(height: 10),
                        const Text(
                          "Tài khoản của bạn",
                          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                        ),
                        const Divider(),

                        // 🧍 Hồ sơ cá nhân
                        ListTile(
                          leading: const Icon(Icons.person, color: Colors.blueAccent),
                          title: const Text("Hồ sơ cá nhân"),
                          onTap: () {
                            Navigator.pop(context); // đóng bottom sheet trước
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => UserProfileView(
                                  userId: widget.userId,
                                  fullName: widget.fullName,
                                ),
                              ),
                            );
                          },
                        ),

                        // 📦 Lịch sử đơn hàng
                        ListTile(
                          leading: const Icon(Icons.receipt_long, color: Colors.green),
                          title: const Text("Lịch sử đơn hàng"),
                          onTap: () {
                            Navigator.pop(context);
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (context) => OrderHistoryView(
                                  userId: widget.userId,
                                ),
                              ),
                            );
                          },
                        ),

                        const SizedBox(height: 10),
                      ],
                    ),
                  );
                },
              );
            },
          ),


          IconButton(
            icon: const Icon(Icons.shopping_cart_outlined, color: Colors.black),
            onPressed: openCart,
          ),

        ],
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // 🔍 Thanh tìm kiếm
            TextField(
              onChanged: filterProducts,
              decoration: InputDecoration(
                hintText: "Tìm bánh bạn muốn...",
                prefixIcon: const Icon(Icons.search),
                filled: true,
                fillColor: const Color(0xFFF6F6F6),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide.none,
                ),
              ),
            ),
            const SizedBox(height: 20),

            //Banner
            SizedBox(
              height: 160,
              child: ClipRRect(
                borderRadius: BorderRadius.circular(16),
                child: const BannerSlider(),
              ),
            ),
            const SizedBox(height: 20),

            //Danh mục
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  "Danh mục",
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                GestureDetector(
                  onTap: showAllCategories,
                  child: const Text(
                    "Xem tất cả",
                    style: TextStyle(color: Color(0xFFF77E6E)),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 10),
            SizedBox(
              height: 90,
              child: ListView.builder(
                scrollDirection: Axis.horizontal,
                itemCount: categories.length,
                itemBuilder: (context, index) {
                  final cat = categories[index] as Map;
                  final catName =
                  (cat['tenLoai'] ?? cat['category']?['tenLoai'] ?? 'Cake').toString();
                  return GestureDetector(
                    onTap: () => openCategory(cat),
                    child: Container(
                      margin: const EdgeInsets.only(right: 15),
                      child: Column(
                        children: [
                          const CircleAvatar(
                            radius: 28,
                            backgroundColor: Color(0xFFFFE7E0),
                            child: Icon(Icons.cake, color: Color(0xFFF77E6E)),
                          ),
                          const SizedBox(height: 5),
                          Text(catName),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
            const SizedBox(height: 20),

            // 🏆 Gợi ý
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: const [
                Text(
                  "Sản phẩm cho bạn",
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                // Text("Xem tất cả", style: TextStyle(color: Color(0xFFF77E6E))),
              ],
            ),
            const SizedBox(height: 10),

            //Danh sách sản phẩm
            GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                mainAxisExtent: 220,
                crossAxisSpacing: 15,
                mainAxisSpacing: 15,
              ),
              itemCount: filteredProducts.length,
              itemBuilder: (context, index) {
                final p = filteredProducts[index] as Map<String, dynamic>;
                final imageUrl = buildImageUrl(p['imageUrl'] ?? p['image'] ?? p['hinhAnh']);
                final name = (p['name'] ?? p['tenSP'] ?? 'Bánh kem').toString();
                final price = (p['price'] ?? p['gia'] ?? 0).toString();

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
                          borderRadius: const BorderRadius.vertical(top: Radius.circular(15)),
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
                          padding: const EdgeInsets.symmetric(horizontal: 8.0),
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
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        backgroundColor: const Color(0xFFF77E6E),
        onPressed: openChat,
        child: const Icon(Icons.chat, color: Colors.white),
      ),
    );
  }
}


/// Trang sản phẩm theo danh mục

class CategoryProductsView extends StatefulWidget {
  final int categoryId;
  final String categoryName;
  final String Function(dynamic) buildImageUrl;
  final String userId;

  const CategoryProductsView({
    super.key,
    required this.categoryId,
    required this.categoryName,
    required this.buildImageUrl,
    required this.userId,
  });

  @override
  State<CategoryProductsView> createState() => _CategoryProductsViewState();
}

class _CategoryProductsViewState extends State<CategoryProductsView> {
  bool isLoading = true;
  List products = [];
  String errorText = '';

  Future<void> _load() async {
    setState(() {
      isLoading = true;
      errorText = '';
    });
    try {
      final data = await ApiService.getProductsByCategory(widget.categoryId);
      setState(() {
        products = data;
        isLoading = false;
      });
    } catch (e) {
      setState(() {
        isLoading = false;
        errorText = "$e";
      });
    }
  }

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  Widget build(BuildContext context) {
    final title = "Danh mục: ${widget.categoryName}";
    return Scaffold(
      appBar: AppBar(
        title: Text(title),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : (errorText.isNotEmpty
          ? Center(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Text(
            "Không tải được sản phẩm.\n$errorText",
            textAlign: TextAlign.center,
          ),
        ),
      )
          : (products.isEmpty
          ? const Center(child: Text("Chưa có sản phẩm trong danh mục này."))
          : Padding(
        padding: const EdgeInsets.all(16),
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
            final imageUrl = widget.buildImageUrl(
              p['imageUrl'] ?? p['image'] ?? p['hinhAnh'],
            );
            final name = (p['name'] ?? p['tenSP'] ?? 'Bánh kem').toString();
            final price = (p['price'] ?? p['gia'] ?? 0).toString();

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
                      borderRadius:
                      const BorderRadius.vertical(top: Radius.circular(15)),
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
                      padding: const EdgeInsets.symmetric(horizontal: 8.0),
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
      ))),
    );
  }
}
