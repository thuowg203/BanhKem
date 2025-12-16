import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../services/api_service.dart';
import 'order_detail_view.dart';

class OrderHistoryView extends StatefulWidget {
  final String userId;
  const OrderHistoryView({super.key, required this.userId});

  @override
  State<OrderHistoryView> createState() => _OrderHistoryViewState();
}

class _OrderHistoryViewState extends State<OrderHistoryView> {
  List<dynamic> orders = [];
  bool isLoading = true;
  String selectedFilter = "Tất cả";

  final List<String> filters = [
    "Tất cả",
    "ChoXacNhan",
    "ChoLayHang",
    "ChoGiaoHang",
    "DaGiao",
    "DaHuy"
  ];

  @override
  void initState() {
    super.initState();
    fetchOrders();
  }

  Future<void> fetchOrders({String? filter}) async {
    setState(() => isLoading = true);
    try {
      // Nếu filter là "Tất cả" thì gửi null để lấy toàn bộ
      final data = await ApiService.getOrderHistory(
        status: (filter == "Tất cả") ? null : filter,
      );
      debugPrint("📦 Dữ liệu đơn hàng: ${data.toString()}");
      setState(() {
        orders = data;
      });
    } catch (e) {
      debugPrint("Lỗi tải đơn hàng: $e");
    } finally {
      setState(() => isLoading = false);
    }
  }

  Future<void> cancelOrder(int id) async {
    try {
      final success = await ApiService.cancelOrder(id);
      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("🗑️ Đã hủy đơn hàng thành công")),
        );
        fetchOrders(filter: selectedFilter);
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Lỗi hủy đơn: $e")),
      );
    }
  }

  String _formatVND(num v) {
    final s = v.toInt().toString().replaceAllMapped(
      RegExp(r'(\d)(?=(\d{3})+(?!\d))'),
          (m) => '${m[1]}.',
    );
    return "$s₫";
  }

  String _statusText(dynamic status) {
    switch (status) {
      case 0:
        return "🟠 Chờ xác nhận";
      case 1:
        return "📦 Chờ lấy hàng";
      case 2:
        return "🚚 Đang giao hàng";
      case 3:
        return "✅ Đã giao";
      case 4:
        return "❌ Đã hủy";
      default:
        return "⚪ Không xác định";
    }
  }

  Color _statusColor(dynamic status) {
    switch (status) {
      case 0:
        return Colors.orange;
      case 1:
        return Colors.amber;
      case 2:
        return Colors.blueAccent;
      case 3:
        return Colors.green;
      case 4:
        return Colors.redAccent;
      default:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Lịch sử đơn hàng"),
        backgroundColor: const Color(0xFFF77E6E),
      ),
      body: Column(
        children: [
          // Bộ lọc
          SizedBox(
            height: 55,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 10),
              itemCount: filters.length,
              itemBuilder: (_, i) {
                final f = filters[i];
                final isActive = f == selectedFilter;
                return Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 4),
                  child: ChoiceChip(
                    label: Text(
                      f == "Tất cả"
                          ? "Tất cả"
                          : f == "ChoXacNhan"
                          ? "Chờ xác nhận"
                          : f == "ChoLayHang"
                          ? "Chờ lấy hàng"
                          : f == "ChoGiaoHang"
                          ? "Đang giao"
                          : f == "DaGiao"
                          ? "Đã giao"
                          : "Đã hủy",
                    ),
                    selected: isActive,
                    onSelected: (_) {
                      setState(() => selectedFilter = f);
                      fetchOrders(filter: f);
                    },
                    selectedColor: const Color(0xFFF77E6E),
                    backgroundColor: Colors.grey[200],
                    labelStyle: TextStyle(
                      color: isActive ? Colors.white : Colors.black87,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                );
              },
            ),
          ),
          const Divider(height: 0),

          // Danh sách đơn hàng
          Expanded(
            child: isLoading
                ? const Center(child: CircularProgressIndicator())
                : orders.isEmpty
                ? const Center(child: Text("Không có đơn hàng nào"))
                : RefreshIndicator(
              onRefresh: () => fetchOrders(filter: selectedFilter),
              child: ListView.builder(
                itemCount: orders.length,
                itemBuilder: (context, i) {
                  final o = orders[i];
                  final id = o["id"]?.toString() ?? "";
                  final date = o["orderDate"]?.toString() ?? "";
                  final status = o["orderStatus"] is int
                      ? o["orderStatus"]
                      : int.tryParse(o["orderStatus"].toString()) ?? -1;
                  final total = (o["totalPrice"] is num) ? o["totalPrice"] : num.tryParse(o["totalPrice"].toString()) ?? 0;
                  final method = o["paymentMethod"]?.toString() ?? "VNPAY";

                  return Card(
                    margin: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                    elevation: 2,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: ListTile(
                      title: Text("Đơn hàng #$id", style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text("Ngày đặt: ${date.toString().substring(0, 10)}"),
                          Text("Tổng tiền: ${_formatVND(total)}"),
                          Text("Thanh toán: $method"),
                          Text(
                            _statusText(status),
                            style: TextStyle(
                              color: _statusColor(status),
                              fontWeight: FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                        ],
                      ),
                      trailing: IconButton(
                        icon: const Icon(Icons.arrow_forward_ios_rounded, size: 18),
                        onPressed: () => Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => OrderDetailView(orderId: int.parse(id)),
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}
