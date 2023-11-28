import { UmbTemporaryFileRepository } from './temporary-file.repository.js';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export type TemporaryFileStatus = 'complete' | 'waiting' | 'error';

export interface TemporaryFileQueueItem {
	id: string;
	file: File;
	status?: TemporaryFileStatus;
	//type?: string;
}

export class UmbTemporaryFileManager {
	#temporaryFileRepository;

	#items = new UmbArrayState<TemporaryFileQueueItem>([], (item) => item.id);
	public readonly items = this.#items.asObservable();

	#isReady = new UmbBooleanState(true);
	public readonly isReady = this.#isReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#temporaryFileRepository = new UmbTemporaryFileRepository(host);
		//this.items.subscribe(() => this.handleQueue());
	}

	uploadOne(id: string, file: File, status: TemporaryFileStatus = 'waiting') {
		this.#items.appendOne({ id, file, status });
		this.handleQueue();
	}

	upload(items: Array<TemporaryFileQueueItem>) {
		this.#items.append(items);
		this.handleQueue();
	}

	removeOne(id: string) {
		this.#items.removeOne(id);
	}

	remove(ids: Array<string>) {
		this.#items.remove(ids);
	}

	private async handleQueue() {
		const items = this.#items.getValue();

		if (!items.length && this.getIsReady()) return;

		this.#isReady.next(false);

		items.forEach(async (item) => {
			if (item.status !== 'waiting') return;

			const { error } = await this.#temporaryFileRepository.upload(item.id, item.file);
			await new Promise((resolve) => setTimeout(resolve, (Math.random() + 0.5) * 1000)); // simulate small delay so that the upload badge is properly shown

			if (error) {
				this.#items.updateOne(item.id, { ...item, status: 'error' });
			} else {
				this.#items.updateOne(item.id, { ...item, status: 'complete' });
			}
		});

		if (!items.find((item) => item.status === 'waiting') && !this.getIsReady()) {
			this.#isReady.next(true);
		}
	}

	getIsReady() {
		return this.#items.getValue();
	}
}
