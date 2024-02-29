import { UmbTemporaryFileRepository } from './temporary-file.repository.js';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export type TemporaryFileStatus = 'complete' | 'waiting' | 'error';

export interface TemporaryFileQueueItem {
	unique: string;
	file: File;
	status?: TemporaryFileStatus;
}

export class UmbTemporaryFileManager extends UmbControllerBase {
	#temporaryFileRepository;

	#queue = new UmbArrayState<TemporaryFileQueueItem>([], (item) => item.unique);
	public readonly queue = this.#queue.asObservable();

	#isReady = new UmbBooleanState(true);
	public readonly isReady = this.#isReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.#temporaryFileRepository = new UmbTemporaryFileRepository(host);
	}

	uploadOne(unique: string, file: File, status: TemporaryFileStatus = 'waiting') {
		this.#queue.appendOne({ unique, file, status });
		this.handleQueue();
	}

	upload(queueItems: Array<TemporaryFileQueueItem>) {
		this.#queue.append(queueItems);
		this.handleQueue();
	}

	removeOne(unique: string) {
		this.#queue.removeOne(unique);
	}

	remove(uniques: Array<string>) {
		this.#queue.remove(uniques);
	}

	private async handleQueue() {
		const queue = this.#queue.getValue();

		if (!queue.length && this.getIsReady()) return;

		this.#isReady.setValue(false);

		queue.forEach(async (item) => {
			if (item.status !== 'waiting') return;

			const { error } = await this.#temporaryFileRepository.upload(item.unique, item.file);
			await new Promise((resolve) => setTimeout(resolve, (Math.random() + 0.5) * 1000)); // simulate small delay so that the upload badge is properly shown

			if (error) {
				this.#queue.updateOne(item.unique, { ...item, status: 'error' });
			} else {
				this.#queue.updateOne(item.unique, { ...item, status: 'complete' });
			}
		});

		if (!queue.find((item) => item.status === 'waiting') && !this.getIsReady()) {
			this.#isReady.setValue(true);
		}
	}

	getIsReady() {
		return this.#queue.getValue();
	}
}
