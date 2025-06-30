import { Component, OnInit } from '@angular/core';
import { Product } from '../../models/product.model';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product';
import { FormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product-form',
  imports: [FormsModule, NgSelectModule, CommonModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.css'
})
export class ProductFormComponent implements OnInit {
  product: Product = { id: 0, name: '', tags: [] };
  imageFile: File | null = null;
  imagePreview: string | null = null;
  isEditMode = false;

  availableTags: string[] = ['tech', 'eco', 'luxury', 'sport', 'office'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEditMode = true;
      this.productService.getById(id).subscribe(p => {
        this.product = p;
      });
    }
  }

  onSubmit(): void {
    const formData = new FormData();
    formData.append('name', this.product.name);
    formData.append('tags', JSON.stringify(this.product.tags));
    if (this.imageFile) {
      formData.append('file', this.imageFile);
    }

    if (this.isEditMode) {
      this.productService.updateWithImage(this.product.id, formData).subscribe(() => {
        this.router.navigate(['/products']);
      });
    } else {
      this.productService.createWithImage(formData).subscribe(() => {
        this.router.navigate(['/products']);
      });
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    this.imageFile = input.files[0];

    // Tạo ảnh preview
    const reader = new FileReader();
    reader.onload = () => {
      this.imagePreview = reader.result as string;
    };
    reader.readAsDataURL(this.imageFile);
  }
}
