import { Component, OnInit } from '@angular/core';
import { Modal } from 'bootstrap';

@Component({
  selector: 'app-create-account',
  templateUrl: './create-account.component.html',
  styleUrls: ['./create-account.component.css'],
})
export class CreateAccountComponent implements OnInit {
  modal!: Modal;

  ngOnInit(): void {
    let createAccountModal = document.getElementById('createAccountModal');
    if (createAccountModal) {
      this.modal = new Modal(createAccountModal);
    }
  }
}
