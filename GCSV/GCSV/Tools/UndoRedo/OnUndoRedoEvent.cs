using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV.Tools.UndoRedo
{
    #region PublicInterface	
    /// <summary>
    /// индерфейс для классов которые будут пискаться
    /// </summary>
    public interface OnUndoRedoEvent
    {
        WriteStepUndoRedo WriteStepUndoRedo { get; set; }
        bool isAsungUR_Operation { get; set; }
    }
    /// <summary>
    /// интерфейс класса с инйормацией об шаге
    /// </summary>
    public interface UndoRedoDataInformation
    {
        CommandEventUR CommandID { get; }
        object objectWrite { get; }
    }
    /// <summary>
    /// интерфейс класса-слушателя
    /// </summary>
    public interface IUndoRedoWorker
    {
        /// <summary>
        /// внимание!!! это делегат обработки данных, именно в него будут приходить данные для обратной вставки в обьект откатов и накатов
        /// первым параметром будет идти обьект слежения прилинкованный к этому экземпляру класса
        /// </summary>
        WriteStepUndoRedo ReparsingDataUndoRedo { get; set; }
        /// <summary>
        /// имя файла в который происходит запись
        /// </summary>
        string FileName { get; }
        bool Init(OnUndoRedoEvent ob, string filename);
        /// <summary>
        /// откат
        /// </summary>
        /// <param name="countSteps">количество шагов</param>
        /// <returns>выполненное количество шагов</returns>
        int Undo(int countSteps = 1);
        /// <summary>
        /// накат
        /// </summary>
        /// <param name="countSteps">количество шагов</param>
        /// <returns>выполненное количество шагов</returns>
        int Redo(int countSteps = 1);
        /// <summary>
        /// количество шаков отката от текущей позиции
        /// </summary>
        int CountUndoForCurrPosition { get; }
        /// <summary>
        /// количество шаков наката от текущей позиции
        /// </summary>
        int CountRedoForCurrPosition { get; }
        /// <summary>
        /// информация по позиции, вид действия, обьект с которым произошло действие
        /// </summary>
        /// <param name="index">номер позиции</param>
        /// <param name="isUndo"></param>
        /// <returns></returns>
        UndoRedoDataInformation StepDataInfo(int index, bool isUndo = true);
        /// <summary>
        /// запись шага
        /// </summary>
        /// <param name="sender">обьект который записывает</param>
        /// <param name="args">параметры</param>
        void WriteStep(object sender, dataUREventArgs args);

    }
    #endregion

    #region Tools

    public enum CommandEventUR
    {
        PropertyChange
    }
    /// <summary>
    /// fhuevtyn tdtynjd
    /// </summary>
    [Serializable]
    public class dataUREventArgs : EventArgs
    {
        object m_CommandId = null;
        object m_objectRewrited = null;
        object m_dataOld = null;
        object m_dataNew = null;
        string m_objectNames = null;
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="CommandId">индификатор комманды (добовляйте свои енумы)</param>
        /// <param name="objectRewrited">обьект в котором произошло изменение (я думаю что сдесь будет гуит парент обьекта)</param>
        /// <param name="objectNames">имя обьекта подверженного изменению (я сюда передаю имя проперти которое изменилось)</param>
        /// <param name="dataOld">старое значение</param>
        /// <param name="dataNew">новое значение</param>
        public dataUREventArgs(object CommandId, object objectRewrited, string objectNames, object dataOld, object dataNew)
        {
            m_CommandId = CommandId;
            m_objectRewrited = objectRewrited;
            m_dataOld = dataOld;
            m_dataNew = dataNew;
            m_objectNames = objectNames;
        }
        /// <summary>
        /// индификатор комманды (добовляйте свои енумы)
        /// </summary>
        public object CommandId
        {
            get { return m_CommandId; }
        }
        /// <summary>
        /// обьект в котором произошло изменение (я думаю что сдесь будет гуит парент обьекта)
        /// </summary>
        public object objectRewrited
        {
            get { return m_objectRewrited; }
        }
        /// <summary>
        /// старое значение
        /// </summary>
        public object dataOld
        {
            get { return m_dataOld; }
        }
        /// <summary>
        /// новое значение
        /// </summary>
        public object dataNew
        {
            get { return m_dataNew; }
        }
        /// <summary>
        /// имя обьекта подверженного изменению (я сюда передаю имя проперти которое изменилось)
        /// </summary>
        public string objectNames
        {
            get { return m_objectNames; }
        }
        /// <summary>
        /// меняетт значениями dataOld и dataNew
        /// </summary>
        internal void ReverceData()
        {
            object o = m_dataNew;
            m_dataNew = m_dataOld;
            m_dataOld = o;
        }
    }
    public delegate void WriteStepUndoRedo(object sender, dataUREventArgs args);
    #endregion

    #region TemplateRealisation
    public class URWorker : SV.Tools.ErrorTracerpt, IUndoRedoWorker
    {
        #region Constructor
        public URWorker(OnUndoRedoEvent ob, string filename, WriteStepUndoRedo ReparsingDataUndoRedo)
        {
            mReparsingDataUndoRedo = ReparsingDataUndoRedo;
            Init(ob, filename);
        }
        #endregion
        #region Destructor
        private bool disposed = false;
        /// <summary>
        /// освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                InitOrFreeChild(m_OnUndoRedoEvent as System.Collections.IEnumerable, false);
                m_OnUndoRedoEvent = null;
                m_fileWork.Close();
                m_fileWork = null;
                disposed = true;
            }
        }
        /// <summary>
        /// деструктор
        /// </summary>
        ~URWorker()
        {
            Dispose(false);
        }
        #endregion


        #region Fields
        /// <summary>
        /// обьект слежения
        /// </summary>
        [NonSerialized]
        OnUndoRedoEvent m_OnUndoRedoEvent;
        UndoRedoTools.UndoRedoClass m_fileWork = new SV.UndoRedoTools.UndoRedoClass();

        string mFileName;
        public string FileName
        {
            get { return mFileName; }
        }
        #endregion

        #region ОбработкаСобытийЗаписи
        public void WriteStep(object sender, dataUREventArgs args)
        {
            if (isUndo)
                return;
            if (m_fileWork == null && !m_fileWork.isOpen)
            {
                Error("Фал записи накатов и откатов не открыт. Запись действий не будет выполненна.");
                return;
            }

            if (args == null)
            {
                Error("Аргумент евента с данными нулевой");
                return;
            }

            m_fileWork.Add(args);
        }
        #endregion

        #region Члены IEstimateUndoRedoWorker


        public virtual bool Init(OnUndoRedoEvent ob, string filename)
        {
            if (ob == null)
                return false;

            m_OnUndoRedoEvent = ob;
            m_OnUndoRedoEvent.WriteStepUndoRedo = this.WriteStep;
            InitOrFreeChild(m_OnUndoRedoEvent as System.Collections.IEnumerable);

            if (m_fileWork.isOpen)
                return true;
            mFileName = filename;
            return m_fileWork.Open(mFileName);
        }
        void InitOrFreeChild(System.Collections.IEnumerable enumerable, bool isInit = true)
        {
            if (enumerable == null)
                return;
            System.Collections.IEnumerator enumer = enumerable.GetEnumerator();

            while (enumer.MoveNext())
            {
                OnUndoRedoEvent ar = enumer.Current as OnUndoRedoEvent;
                if (ar != null)
                {
                    if (isInit)
                        ar.WriteStepUndoRedo = this.WriteStep;
                    else
                        ar.WriteStepUndoRedo = null;
                }
                InitOrFreeChild(enumer.Current as System.Collections.IEnumerable);

            }

        }

        bool isUndo = false;
        object locker = new object();
        int OperationUR(int countSteps = 1, bool undo = true)
        {
            lock (locker)
            {
                isUndo = true;
                if (m_fileWork == null && !m_fileWork.isOpen)
                    return -1;
                for (int a = 0; a < countSteps; a++)
                {
                    object ob = new object();
                    if (undo)
                    {
                        if (!m_fileWork.Undo(ref ob))
                            return a;
                    }
                    else
                    {
                        if (!m_fileWork.Redo(ref ob))
                            return a;
                        else
                        {
                            dataUREventArgs o = ob as dataUREventArgs;
                            if (o != null)
                            {
                                o.ReverceData();
                            }
                        }
                    }
                    if (mReparsingDataUndoRedo != null)
                        mReparsingDataUndoRedo(m_OnUndoRedoEvent, ob as dataUREventArgs);
                }
                isUndo = false;
            }
            return countSteps;
        }

        public int Undo(int countSteps = 1)
        {

            return OperationUR(countSteps);
        }

        public int Redo(int countSteps = 1)
        {
            return OperationUR(countSteps, false);
        }

        public int CountUndoForCurrPosition
        {
            get { throw new NotImplementedException(); }
        }

        public int CountRedoForCurrPosition
        {
            get { throw new NotImplementedException(); }
        }

        public UndoRedoDataInformation StepDataInfo(int index, bool isUndo = true)
        {
            throw new NotImplementedException();
        }

        WriteStepUndoRedo mReparsingDataUndoRedo;
        /// <summary>
        /// внимание!!! это делегат обработки данных, именно в него будут приходить данные для обратной вставки в обьект откатов и накатов
        /// первым параметром будет идти обьект слежения прилинкованный к этому экземпляру класса
        /// </summary>
        public WriteStepUndoRedo ReparsingDataUndoRedo
        {
            get
            {
                return mReparsingDataUndoRedo;
            }
            set
            {
                mReparsingDataUndoRedo = value;
            }
        }
        #endregion
    }
    #endregion
}
