import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbOutlineStyleController extends UmbControllerBase {
	#hideFocusOutline = false;

	override hostConnected(): void {
		super.hostConnected();

		const host = this.getHostElement();
		host.addEventListener('pointerdown', this.#onPointerDown);
		host.addEventListener('keydown', this.#onKeyDown as EventListener, { capture: true });
	}

	readonly #onPointerDown = () => {
		if (!this.#hideFocusOutline) {
			document.body.style.setProperty('--uui-show-focus-outline', '0');
			this.#hideFocusOutline = true;
		}
	};
	readonly #onKeyDown = (e: KeyboardEvent) => {
		// If tab or shift tab is pressed, we want to show the focus outline, but only if the last interaction was not a pointer event.
		if (e.key !== 'Tab') return;

		// If the last interaction was a pointer event, we don't want to show the focus outline.
		if (this.#hideFocusOutline) {
			document.body.style.removeProperty('--uui-show-focus-outline');
			this.#hideFocusOutline = false;
		}
	};

	override hostDisconnected(): void {
		super.hostDisconnected();
		document.body.style.removeProperty('--uui-show-focus-outline');

		const host = this.getHostElement();
		host.removeEventListener('pointerdown', this.#onPointerDown);
		host.removeEventListener('keydown', this.#onKeyDown as EventListener, { capture: true });
	}
}
