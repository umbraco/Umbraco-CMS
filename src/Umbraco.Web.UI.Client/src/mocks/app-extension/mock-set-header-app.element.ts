import { umbMockManager } from '../mock-manager.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const AVAILABLE_MOCK_SETS = ['default', 'kenn', 'test'] as const;
const MOCK_SET_STORAGE_KEY = 'umb:mockSet';

@customElement('mock-set-header-app')
export class MockSetHeaderAppElement extends UmbLitElement {
	@state()
	private _currentSet: string = umbMockManager.currentSetName;

	#onSetSelected(setName: string) {
		if (setName === this._currentSet) return;

		localStorage.setItem(MOCK_SET_STORAGE_KEY, setName);
		window.location.reload();
	}

	override render() {
		return html`
			<uui-button compact label="Mock data set" look="primary" popovertarget="mock-set-popover">
				Mock: ${this._currentSet}
			</uui-button>
			<uui-popover-container id="mock-set-popover" placement="bottom-start">
				<umb-popover-layout>
					<div class="mock-set-list">
						${AVAILABLE_MOCK_SETS.map(
							(setName) => html`
								<uui-menu-item
									label=${setName}
									?active=${setName === this._currentSet}
									@click=${() => this.#onSetSelected(setName)}>
								</uui-menu-item>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			uui-button {
				text-wrap: nowrap;
				--uui-button-background-color: transparent;
				--uui-button-background-color-hover: var(--uui-color-emphasis);
			}

			.mock-set-list {
				min-width: 120px;
				--uui-menu-item-indent: 0;
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

export { MockSetHeaderAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'mock-set-header-app': MockSetHeaderAppElement;
	}
}
