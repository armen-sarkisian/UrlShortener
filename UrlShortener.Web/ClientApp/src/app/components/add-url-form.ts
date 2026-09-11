import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

/** The "Add new Url" section. Shown to authenticated users only; the parent decides that. */
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

  /** The parent clears the field only after the server confirms the change. */
  reset(): void {
    this.form.reset();
  }
}
