import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

/** Секция "Add new Url". Показывается только авторизованным — этим управляет родитель. */
@Component({
  selector: 'app-add-url-form',
  imports: [ReactiveFormsModule],
  templateUrl: './add-url-form.html',
  styleUrl: './add-url-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddUrlForm {
  readonly pending = input(false);
  readonly submitted = output<string>();

  protected readonly form = new FormGroup({
    url: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected submit(): void {
    if (this.form.invalid || this.pending()) {
      return;
    }

    this.submitted.emit(this.form.getRawValue().url.trim());
  }

  /** Родитель очищает поле только после успешного ответа сервера. */
  reset(): void {
    this.form.reset();
  }
}
