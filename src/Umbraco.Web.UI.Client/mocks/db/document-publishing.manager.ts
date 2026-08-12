import type { UmbMockDocumentModel } from '../data/mock-data-set.types.js';
import type { UmbDocumentMockDB } from './document.db.js';
import type {
	CreateAndPublishDocumentRequestModel,
	PublishDocumentRequestModel,
	PublishDocumentWithDescendantsRequestModel,
	UnpublishDocumentRequestModel,
	UpdateAndPublishDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export class UmbMockDocumentPublishingManager {
	#documentDb: UmbDocumentMockDB;
	#tasks = new Map<
		string,
		{
			task: () => Promise<void>;
			isComplete: boolean;
		}
	>();

	constructor(documentDb: UmbDocumentMockDB) {
		this.#documentDb = documentDb;
	}

	publish(id: string, data: PublishDocumentRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

		data.publishSchedules.forEach((culture) => {
			const publishTime = culture.schedule?.publishTime;
			const unpublishTime = culture.schedule?.unpublishTime;

			if (publishTime && new Date(publishTime) < new Date()) {
				throw new Error('Publish date cannot be in the past');
			}

			if (unpublishTime && new Date(unpublishTime) < new Date()) {
				throw new Error('Unpublish date cannot be in the past');
			}

			if (unpublishTime && publishTime && new Date(unpublishTime) < new Date(publishTime)) {
				throw new Error('Unpublish date cannot be before publish date');
			}

			const variant = document.variants.find((x) => x.culture === culture.culture);
			if (variant) {
				variant.state = 'Published' as UmbDocumentVariantState;
				variant.scheduledPublishDate = publishTime;
				variant.scheduledUnpublishDate = unpublishTime;
				variant.updateDate = new Date().toISOString();
			}
		});

		this.#documentDb.detail.update(id, document);
	}

	createAndPublish(data: CreateAndPublishDocumentRequestModel) {
		const id = this.#documentDb.detail.create(data);
		this.#publishCultures(id, data.culturesToPublish);
		return id;
	}

	updateAndPublish(id: string, data: UpdateAndPublishDocumentRequestModel) {
		this.#documentDb.detail.update(id, data);
		this.#publishCultures(id, data.culturesToPublish);
	}

	#publishCultures(id: string, culturesToPublish: Array<string>) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

		// Invariant content types publish with an empty cultures array; publish the invariant variant in that case.
		const cultures: Array<string | null> = culturesToPublish.length > 0 ? culturesToPublish : [null];

		cultures.forEach((culture) => {
			const variant = document.variants.find((x) => x.culture === culture);
			if (variant) {
				variant.state = 'Published' as UmbDocumentVariantState;
				variant.updateDate = new Date().toISOString();
			}
		});

		this.#documentDb.detail.update(id, document);
	}

	publishWithDescendants(id: string, data: PublishDocumentWithDescendantsRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);
		const documents = this.getDescendants(id, []);

		const taskId = UmbId.new();
		const task = async () => {
			for (const culture of data.cultures) {
				for (const d of documents) {
					const variant = document.variants.find((x) => x.culture === culture);
					if (variant && (data.includeUnpublishedDescendants || variant.state !== 'Published')) {
						variant.state = 'Published' as UmbDocumentVariantState;
						variant.updateDate = new Date().toISOString();
					}
					this.#documentDb.detail.update(d.id, d);
				}
			}
		};

		this.#tasks.set(taskId, {
			task,
			isComplete: false,
		});
		const taskPromise = task();
		taskPromise
			.then(() => {
				this.#tasks.set(taskId, {
					task: () => taskPromise,
					isComplete: true,
				});
			})
			.catch(() => {
				this.#tasks.set(taskId, {
					task: () => taskPromise,
					isComplete: true,
				});
			});

		return taskId;
	}

	taskResult(taskId: string) {
		const task = this.#tasks.get(taskId);
		if (!task) {
			throw new Error(`Task with id ${taskId} not found`);
		}

		return task.isComplete;
	}

	getDescendants(id: string, documents: UmbMockDocumentModel[]) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);
		documents.push(document);
		const descendants = this.#documentDb.tree.getChildrenOf({ parentId: document.id, skip: 0, take: 10000 });
		descendants.items.forEach((d) => {
			this.getDescendants(d.id, documents);
		});
		return documents;
	}

	unpublish(id: string, data: UnpublishDocumentRequestModel) {
		const document: UmbMockDocumentModel = this.#documentDb.detail.read(id);

		if (data.cultures) {
			data.cultures.forEach((culture) => {
				const variant = document.variants.find((x) => x.culture === culture);

				if (variant) {
					variant.state = 'Draft' as UmbDocumentVariantState;
					variant.scheduledPublishDate = null;
					variant.scheduledUnpublishDate = null;
					variant.updateDate = new Date().toISOString();
				}
			});
		} else {
			document.variants.forEach((variant) => {
				variant.state = 'Draft' as UmbDocumentVariantState;
				variant.scheduledPublishDate = null;
				variant.scheduledUnpublishDate = null;
				variant.updateDate = new Date().toISOString();
			});
		}

		this.#documentDb.detail.update(id, document);
	}
}
