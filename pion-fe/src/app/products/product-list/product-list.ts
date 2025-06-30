import { Component, OnInit } from '@angular/core';
import { Product } from '../../models/product.model';
import { ProductService } from '../../services/product';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { environment } from '../../../environments/environment';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  searchKeyword: string = '';

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  getImageUrl(relativeUrl: string): string {
    return `${environment.baseUrl}${relativeUrl}`;
  }


  loadProducts(): void {
    this.productService.getAll().subscribe(data => {
      this.products = data;
    });
  }

  deleteProduct(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này không?')) {
      this.productService.delete(id).subscribe(() => {
        this.loadProducts();
      });
    }
  }

  onSearch(): void {
    this.productService.search(this.searchKeyword).subscribe(data => {
      this.products = data;
    });
  }

  clearSearch(): void {
    this.searchKeyword = '';
    this.loadProducts();
  }
}
