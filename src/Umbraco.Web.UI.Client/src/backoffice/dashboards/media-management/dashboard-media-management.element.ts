import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import '../../components/collection/collection-media.element';
import { BehaviorSubject, Observable } from 'rxjs';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbMediaStore } from '@umbraco-cms/stores/media/media.store';

@customElement('umb-dashboard-media-management')
export class UmbDashboardMediaManagementElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}
		`,
	];

	@property()
	public entityKey = '';

	private _mediaStore?: UmbMediaStore;

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	private _mediaItems: BehaviorSubject<Array<MediaDetails>> = new BehaviorSubject(<Array<MediaDetails>>[]);
	public readonly mediaItems: Observable<Array<MediaDetails>> = this._mediaItems.asObservable();

	private _search: BehaviorSubject<string> = new BehaviorSubject('');
	public readonly search: Observable<string> = this._search.asObservable();

	constructor() {
		super();
		this.provideContext('umbMediaContext', this);
		this.consumeAllContexts(['umbMediaStore'], (instance) => {
			this._mediaStore = instance['umbMediaStore'];
			this._observeMediaItems();
		});
	}

	private _observeMediaItems() {
		if (!this._mediaStore) return;

		if (this.entityKey) {
			this.observe<Array<MediaDetails>>(this._mediaStore?.getTreeItemChildren(this.entityKey), (items) => {
				this._mediaItems.next(items);
			});
		} else {
			this.observe<Array<MediaDetails>>(this._mediaStore?.getTreeRoot(), (items) => {
				this._mediaItems.next(items);
			});
		}
	}

	public setSearch(value: string) {
		if (!value) value = '';

		this._search.next(value);
		this._observeMediaItems();
		this.requestUpdate('search');
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this._selection.next(value);
		this.requestUpdate('selection');
	}

	public select(key: string) {
		const selection = this._selection.getValue();
		this._selection.next([...selection, key]);
		this.requestUpdate('selection');
	}

	public deselect(key: string) {
		const selection = this._selection.getValue();
		this._selection.next(selection.filter((k) => k !== key));
		this.requestUpdate('selection');
	}

	render() {
		return html`<umb-collection-media entityType="media"></umb-collection-media>`;
	}
}

export default UmbDashboardMediaManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-media-management': UmbDashboardMediaManagementElement;
	}
}
