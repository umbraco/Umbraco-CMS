import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbDocumentRepository } from '../repository/document.repository';
import type { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbDocumentTypeRepository } from '../../document-types/repository/document-type.repository';
import type { DocumentModel, DocumentTypeModel } from '@umbraco-cms/backend-api';
import {
	partialUpdateFrozenArray,
	ObjectState,
	ArrayState,
	UmbObserverController,
	appendToFrozenArray,
} from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

// TODO: should this context be called DocumentDraft instead of workspace? or should the draft be part of this?

type EntityType = DocumentModel;
export class UmbDocumentWorkspaceContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#isNew = false;
	#host: UmbControllerHostInterface;
	#documentRepository: UmbDocumentRepository;
	#documentTypeRepository: UmbDocumentTypeRepository;
	//#dataTypeRepository: UmbDataTypeRepository;

	#data = new ObjectState<EntityType | undefined>(undefined);
	documentTypeKey = this.#data.getObservablePart((data) => data?.contentTypeKey);

	#documentTypes = new ArrayState<DocumentTypeModel>([], (x) => x.key);
	documentTypes = this.#documentTypes.asObservable();

	constructor(host: UmbControllerHostInterface) {
		super(host);
		this.#host = host;
		this.#documentRepository = new UmbDocumentRepository(this.#host);
		this.#documentTypeRepository = new UmbDocumentTypeRepository(this.#host);
		//this.#dataTypeRepository = new UmbDataTypeRepository(this.#host);

		new UmbObserverController(this._host, this.documentTypeKey, (key) => this.loadDocumentType(key));
	}

	async load(entityKey: string) {
		const { data } = await this.#documentRepository.requestByKey(entityKey);
		if (data) {
			this.#isNew = false;
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.#documentRepository.createScaffold(parentKey);
		if (!data) return;
		this.#isNew = true;
		this.#data.next(data);
	}

	async loadDocumentType(key?: string) {
		if (!key) return;

		const { data } = await this.#documentTypeRepository.requestByKey(key);
		if (!data) return;

		// Load inherited and composed types:
		await data?.compositions?.forEach(async (composition) => {
			if (composition.key) {
				this.loadDocumentType(composition.key);
			}
		});

		new UmbObserverController(this._host, await this.#documentTypeRepository.byKey(key), (data) => {
			if (data) {
				this.#documentTypes.appendOne(data);
				this.loadDataTypeOfDocumentType(data);
			}
		});
	}

	async loadDataTypeOfDocumentType(documentType?: DocumentTypeModel) {
		if (!documentType) return;

		// Load inherited and composed types:
		await documentType?.properties?.forEach(async (property) => {
			if (property.dataTypeKey) {
				this.loadDataType(property.dataTypeKey);
			}
		});
	}

	async loadDataType(key?: string) {
		if (!key) return;

		//const { data } = await this.#dataTypeRepository.requestDetails(key);

		/*new UmbObserverController(this._host, await this.#documentTypeRepository.byKey(key), (data) => {
			if (data) {
				this.#documentTypes.appendOne(data);
			}
		});*/
	}

	getData() {
		return this.#data.getValue();
	}

	/*
	getUnique() {
		return this.#data.getKey();
	}
	*/

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'document';
	}

	setName(name: string, culture?: string | null, segment?: string | null) {
		const variants = this.#data.getValue()?.variants || [];
		const newVariants = partialUpdateFrozenArray(
			variants,
			{ name },
			(v) => v.culture == culture && v.segment == segment
		);
		this.#data.update({ variants: newVariants });
	}
	/*
	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;

	*/

	/**
	 * Concept for Repository impl.:

	load(entityKey: string) {
		this.#repository.load(entityKey).then((data) => {
			this.#draft.next(data)
		})
	}

	create(parentKey: string | undefined) {
		this.#repository.create(parentKey).then((data) => {
			this.#draft.next(data)
		})
	}
	*/

	propertiesOf(culture: string | null, segment: string | null) {
		return this.#data.getObservablePart((data) =>
			data?.properties?.filter((p) => (culture === p.culture || null) && (segment === p.segment || null))
		);
	}

	propertyStructure() {
		// TODO: handle composition of document types.
		return this.#documentTypes.getObservablePart((data) => data[0]?.properties);
	}

	setPropertyValue(alias: string, value: unknown) {
		const entry = { alias: alias, value: value };

		const currentData = this.#data.value;
		if (currentData) {
			// TODO: make a partial update method for array of data, (idea/concept, use if this case is getting common)
			const newDataSet = appendToFrozenArray(currentData.properties || [], entry, (x) => x.alias);
			this.#data.update({ properties: newDataSet });
		}
	}

	async save() {
		if (!this.#data.value) return;
		if (this.#isNew) {
			await this.#documentRepository.create(this.#data.value);
		} else {
			await this.#documentRepository.save(this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.#isNew = false;
	}

	async delete(key: string) {
		await this.#documentRepository.delete(key);
	}

	/*
	concept notes:

	public saveAndPublish() {

	}

	public saveAndPreview() {

	}
	*/

	public destroy(): void {
		this.#data.complete();
	}
}
