import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbOutlineStyleController extends UmbControllerBase {
	#hadMouseDown = false;

	override hostConnected(): void {
		super.hostConnected();

		const host = this.getHostElement();
		host.addEventListener('focusout', this.#onFocusOut);
		host.addEventListener('mousedown', this.#onMouseDown);
		host.addEventListener('mouseup', this.#onMouseUp);
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
		super.hostDisconnected();

		const host = this.getHostElement();
		host.removeEventListener('focusout', this.#onFocusOut);
		host.removeEventListener('mousedown', this.#onMouseDown);
		host.removeEventListener('mouseup', this.#onMouseUp);
	}
}
