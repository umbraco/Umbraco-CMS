import { MOCK_USER_STORAGE_KEY, umbMockManager } from '../mock-manager.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('mock-user-header-app')
export class MockUserHeaderAppElement extends UmbLitElement {
	@state()
	private _currentUserName = umbMockManager.currentUserName;

	#onUserSelected(id: string) {
		localStorage.setItem(MOCK_USER_STORAGE_KEY, id);
		window.location.reload();
	}

	override render() {
		if (!umbMockManager.canSwitchUser) return nothing;

		return html`
			<uui-button compact label="Mock user" look="primary" popovertarget="mock-user-popover">
				User: ${this._currentUserName}
			</uui-button>
			<uui-popover-container id="mock-user-popover" placement="bottom-start">
				<umb-popover-layout>
					<div class="mock-user-list">
						${umbMockManager.availableUsers.map(
							({ id, name }) => html`
								<uui-menu-item
									label=${name}
									?active=${name === this._currentUserName}
									@click=${() => this.#onUserSelected(id)}>
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

			.mock-user-list {
				min-width: 240px;
				--uui-menu-item-indent: 0;
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

export { MockUserHeaderAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'mock-user-header-app': MockUserHeaderAppElement;
	}
}
