import { UmbUfmRenderElement } from '../components/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Renders a UFM
 */
export class UmbUfmVirtualRenderController extends UmbControllerBase {
	#element: UmbUfmRenderElement;

	set markdown(markdown: string | undefined) {
		this.#element.markdown = markdown;
	}
	get markdown(): string | undefined {
		return this.#element.markdown;
	}
	set value(value: unknown | undefined) {
		this.#element.value = value;
	}
	get value(): unknown | undefined {
		return this.#element.value;
	}

	constructor(host: UmbControllerHost) {
		super(host);

		const element = new UmbUfmRenderElement();
		element.inline = true;
		element.style.visibility = 'hidden';
		console.log(this.getHostElement());
		this.getHostElement().appendChild(element);
		this.#element = element;
	}

	override hostConnected(): void {}

	getAsText(): string {
		return this.#element.getAsText();
	}

	override destroy(): void {
		super.destroy();
		this.#element?.destroy();
		(this.#element as any) = undefined;
	}
}
