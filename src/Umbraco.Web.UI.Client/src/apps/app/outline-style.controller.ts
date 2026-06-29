import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbOutlineStyleController extends UmbControllerBase {
	#hadMouseDown = false;

	override hostConnected(): void {
		super.hostConnected();

		this.addEventListener('focusout', this.#onFocusOut);
		this.addEventListener('mousedown', this.#onMouseDown);
		this.addEventListener('mouseup', this.#onMouseUp);
	}

	#onFocusOut = () => {
		if (this.#hadMouseDown === false) {
			document.body.style.removeProperty('--uui-show-focus-outline');
		}
		this.#hadMouseDown = false;
	};

	#onMouseDown = () => {
		document.body.style.setProperty('--uui-show-focus-outline', '0');
		this.#hadMouseDown = true;
	};

	#onMouseUp = () => {
		this.#hadMouseDown = false;
	};

	override hostDisconnected(): void {
		this.removeEventListener('focusout', this.#onFocusOut);
		this.removeEventListener('mousedown', this.#onMouseDown);
		this.removeEventListener('mouseup', this.#onMouseUp);
	}
}
