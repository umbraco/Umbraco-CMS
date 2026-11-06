import { getSelectedExampleNames, UMB_EXAMPLES_STORAGE_KEY } from '../examples.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const EXAMPLE_PATH_PREFIX = 'examples/';

const AVAILABLE_EXAMPLES = Object.keys(import.meta.glob('../../examples/*/index.ts'))
	.map((path) => path.split('/').at(-2)!)
	.sort((a, b) => a.localeCompare(b));

const LOCKED_EXAMPLE = import.meta.env.VITE_EXAMPLE_PATH?.startsWith(EXAMPLE_PATH_PREFIX)
	? import.meta.env.VITE_EXAMPLE_PATH.slice(EXAMPLE_PATH_PREFIX.length)
	: undefined;

@customElement('mock-examples-header-app')
// eslint-disable-next-line @typescript-eslint/naming-convention
export class MockExamplesHeaderAppElement extends UmbLitElement {
	readonly #bootSelection = getSelectedExampleNames();

	@state()
	private _selection: Array<string> = [...this.#bootSelection];

	get #loadedCount() {
		return new Set([...this.#bootSelection, ...(LOCKED_EXAMPLE ? [LOCKED_EXAMPLE] : [])]).size;
	}

	get #hasChanges() {
		const selected = new Set(this._selection);
		const boot = new Set(this.#bootSelection);
		return selected.size !== boot.size || [...selected].some((name) => !boot.has(name));
	}

	#onToggle(name: string, checked: boolean) {
		this._selection = checked ? [...this._selection, name] : this._selection.filter((n) => n !== name);
		localStorage.setItem(UMB_EXAMPLES_STORAGE_KEY, JSON.stringify(this._selection));
	}

	#onReset() {
		localStorage.removeItem(UMB_EXAMPLES_STORAGE_KEY);
		window.location.reload();
	}

	#onLoad() {
		window.location.reload();
	}

	override render() {
		return html`
			<uui-button id="btn" compact label="Examples" look="primary" popovertarget="examples-popover">
				${when(
					this.#loadedCount,
					() => html`Examples: ${this.#loadedCount}`,
					() => html`Examples`,
				)}
			</uui-button>
			<uui-popover-container id="examples-popover" placement="bottom-start">
				<umb-popover-layout>
					<umb-body-layout>
						<div class="examples-list">
							${AVAILABLE_EXAMPLES.map((name) => {
								const locked = name === LOCKED_EXAMPLE;
								return html`
									<uui-checkbox
										label=${name}
										?disabled=${locked}
										?checked=${locked || this._selection.includes(name)}
										@change=${(e: Event) =>
											this.#onToggle(name, (e.target as HTMLInputElement).checked)}></uui-checkbox>
								`;
							})}
						</div>
						<uui-button slot="actions" compact label="Reset" @click=${this.#onReset}></uui-button>
						<uui-button
							slot="actions"
							compact
							color="positive"
							look="primary"
							label="Load extensions"
							?disabled=${!this.#hasChanges}
							@click=${this.#onLoad}></uui-button>
					</umb-body-layout>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override readonly styles = [
		css`
			#btn {
				text-wrap: nowrap;
				--uui-button-background-color: transparent;
				--uui-button-background-color-hover: var(--uui-color-emphasis);
			}

			.examples-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-2);

				max-height: 320px;
			}
		`,
	];
}

export { MockExamplesHeaderAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'mock-examples-header-app': MockExamplesHeaderAppElement;
	}
}
