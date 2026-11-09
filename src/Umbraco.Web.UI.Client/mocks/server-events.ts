import type { UmbMockDataSet } from './data/mock-data-set.types.js';

export type UmbMockServerEventType = 'Created' | 'Updated' | 'Deleted';

export interface UmbMockServerEvent {
	eventSource: string;
	eventType: UmbMockServerEventType;
	key: string;
}

const EVENT_SOURCES: Partial<Record<keyof UmbMockDataSet, string>> = {
	dataType: 'Umbraco:CMS:DataType',
	dictionary: 'Umbraco:CMS:DictionaryItem',
	document: 'Umbraco:CMS:Document',
	documentBlueprint: 'Umbraco:CMS:DocumentBlueprint',
	documentType: 'Umbraco:CMS:DocumentType',
	language: 'Umbraco:CMS:Language',
	media: 'Umbraco:CMS:Media',
	mediaType: 'Umbraco:CMS:MediaType',
	member: 'Umbraco:CMS:Member',
	memberGroup: 'Umbraco:CMS:MemberGroup',
	memberType: 'Umbraco:CMS:MemberType',
	partialView: 'Umbraco:CMS:PartialView',
	script: 'Umbraco:CMS:Script',
	staticFile: 'Umbraco:CMS:StaticFile',
	stylesheet: 'Umbraco:CMS:Stylesheet',
	template: 'Umbraco:CMS:Template',
	user: 'Umbraco:CMS:User',
	userGroup: 'Umbraco:CMS:UserGroup',
	webhook: 'Umbraco:CMS:Webhook',
};

type Listener = (event: UmbMockServerEvent) => void;

class UmbMockServerEvents {
	#listeners = new Set<Listener>();

	emit(dataKey: keyof UmbMockDataSet, eventType: UmbMockServerEventType, key: string) {
		const eventSource = EVENT_SOURCES[dataKey];
		if (!eventSource) return;

		const event: UmbMockServerEvent = { eventSource, eventType, key };
		this.#listeners.forEach((listener) => listener(event));
	}

	subscribe(listener: Listener) {
		this.#listeners.add(listener);
		return () => {
			this.#listeners.delete(listener);
		};
	}
}

export const umbMockServerEvents = new UmbMockServerEvents();
